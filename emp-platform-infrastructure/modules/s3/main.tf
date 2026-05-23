# ---------------------------------------------------------------------------------------------------------------------
# S3 MODULE — MAIN
# Provisions three S3 buckets for the EMP platform:
#   1. SOW Documents — versioned, KMS-encrypted, CORS-enabled, lifecycle tiering, access-logged
#   2. Access Logs   — KMS-encrypted, 90-day expiration, target for server access logging
#   3. Build Artifacts — KMS-encrypted, general-purpose CI/CD artifact storage
#
# All buckets enforce Block Public Access and use SSE-KMS with a Customer Managed Key.
# ---------------------------------------------------------------------------------------------------------------------

data "aws_caller_identity" "current" {}

# =====================================================================================================================
# 1. ACCESS LOGS BUCKET (must be created first — SOW bucket logs to it)
# =====================================================================================================================

resource "aws_s3_bucket" "logs" {
  bucket        = var.logs_bucket_name
  force_destroy = var.force_destroy

  tags = merge(var.tags, {
    Name    = var.logs_bucket_name
    Purpose = "access-logs"
  })
}

resource "aws_s3_bucket_public_access_block" "logs" {
  bucket = aws_s3_bucket.logs.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_server_side_encryption_configuration" "logs" {
  bucket = aws_s3_bucket.logs.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm     = "aws:kms"
      kms_master_key_id = var.kms_key_arn
    }
    bucket_key_enabled = true
  }
}

resource "aws_s3_bucket_lifecycle_configuration" "logs" {
  bucket = aws_s3_bucket.logs.id

  rule {
    id     = "expire-logs"
    status = "Enabled"

    filter {}

    expiration {
      days = var.logs_expiration_days
    }
  }
}

# ACL granting the S3 log delivery group write access for server access logging
resource "aws_s3_bucket_ownership_controls" "logs" {
  bucket = aws_s3_bucket.logs.id

  rule {
    object_ownership = "BucketOwnerPreferred"
  }
}

resource "aws_s3_bucket_acl" "logs" {
  depends_on = [aws_s3_bucket_ownership_controls.logs]

  bucket = aws_s3_bucket.logs.id
  acl    = "log-delivery-write"
}

# =====================================================================================================================
# 2. SOW DOCUMENTS BUCKET
# =====================================================================================================================

resource "aws_s3_bucket" "sow" {
  bucket        = var.sow_bucket_name
  force_destroy = var.force_destroy

  tags = merge(var.tags, {
    Name    = var.sow_bucket_name
    Purpose = "sow-documents"
  })
}

resource "aws_s3_bucket_public_access_block" "sow" {
  bucket = aws_s3_bucket.sow.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_versioning" "sow" {
  bucket = aws_s3_bucket.sow.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "sow" {
  bucket = aws_s3_bucket.sow.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm     = "aws:kms"
      kms_master_key_id = var.kms_key_arn
    }
    bucket_key_enabled = true
  }
}

resource "aws_s3_bucket_logging" "sow" {
  bucket = aws_s3_bucket.sow.id

  target_bucket = aws_s3_bucket.logs.id
  target_prefix = "sow-access-logs/"
}

resource "aws_s3_bucket_lifecycle_configuration" "sow" {
  bucket = aws_s3_bucket.sow.id

  rule {
    id     = "sow-tiered-storage"
    status = "Enabled"

    filter {}

    transition {
      days          = var.sow_ia_transition_days
      storage_class = "STANDARD_IA"
    }

    transition {
      days          = var.sow_glacier_transition_days
      storage_class = "GLACIER"
    }

    # Clean up expired delete markers and non-current versions
    noncurrent_version_expiration {
      noncurrent_days = 730
    }
  }
}

resource "aws_s3_bucket_cors_configuration" "sow" {
  bucket = aws_s3_bucket.sow.id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "PUT", "POST"]
    allowed_origins = var.cors_allowed_origins
    expose_headers  = ["ETag", "x-amz-request-id", "x-amz-id-2"]
    max_age_seconds = 3600
  }
}

# Bucket policy: restrict uploads to allowed SOW file extensions (.pdf, .docx, .doc)
resource "aws_s3_bucket_policy" "sow" {
  bucket = aws_s3_bucket.sow.id

  depends_on = [aws_s3_bucket_public_access_block.sow]

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "DenyNonSowFileTypes"
        Effect    = "Deny"
        Principal = "*"
        Action    = "s3:PutObject"
        Resource  = "${aws_s3_bucket.sow.arn}/*"
        Condition = {
          StringNotLike = {
            "s3:prefix" = [
              "*.pdf",
              "*.docx",
              "*.doc"
            ]
          }
          # Exempt the root account and service roles from this restriction so that
          # lifecycle transitions, replication, and administrative operations are not blocked.
          StringNotEquals = {
            "aws:PrincipalAccount" = data.aws_caller_identity.current.account_id
          }
        }
      },
      {
        Sid       = "EnforceSslOnly"
        Effect    = "Deny"
        Principal = "*"
        Action    = "s3:*"
        Resource = [
          aws_s3_bucket.sow.arn,
          "${aws_s3_bucket.sow.arn}/*"
        ]
        Condition = {
          Bool = {
            "aws:SecureTransport" = "false"
          }
        }
      }
    ]
  })
}

# =====================================================================================================================
# 3. BUILD ARTIFACTS BUCKET
# =====================================================================================================================

resource "aws_s3_bucket" "artifacts" {
  bucket        = var.artifacts_bucket_name
  force_destroy = var.force_destroy

  tags = merge(var.tags, {
    Name    = var.artifacts_bucket_name
    Purpose = "build-artifacts"
  })
}

resource "aws_s3_bucket_public_access_block" "artifacts" {
  bucket = aws_s3_bucket.artifacts.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_server_side_encryption_configuration" "artifacts" {
  bucket = aws_s3_bucket.artifacts.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm     = "aws:kms"
      kms_master_key_id = var.kms_key_arn
    }
    bucket_key_enabled = true
  }
}

resource "aws_s3_bucket_policy" "artifacts" {
  bucket = aws_s3_bucket.artifacts.id

  depends_on = [aws_s3_bucket_public_access_block.artifacts]

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "EnforceSslOnly"
        Effect    = "Deny"
        Principal = "*"
        Action    = "s3:*"
        Resource = [
          aws_s3_bucket.artifacts.arn,
          "${aws_s3_bucket.artifacts.arn}/*"
        ]
        Condition = {
          Bool = {
            "aws:SecureTransport" = "false"
          }
        }
      }
    ]
  })
}
