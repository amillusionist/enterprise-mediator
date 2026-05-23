# ---------------------------------------------------------------------------------------------------------------------
# S3 MODULE — OUTPUTS
# Exports bucket identifiers and domain names for consumption by other modules and services.
# ---------------------------------------------------------------------------------------------------------------------

# SOW Documents Bucket
output "sow_bucket_id" {
  description = "The name (ID) of the SOW documents bucket."
  value       = aws_s3_bucket.sow.id
}

output "sow_bucket_arn" {
  description = "The ARN of the SOW documents bucket."
  value       = aws_s3_bucket.sow.arn
}

output "sow_bucket_domain_name" {
  description = "The bucket domain name of the SOW documents bucket."
  value       = aws_s3_bucket.sow.bucket_domain_name
}

# Access Logs Bucket
output "logs_bucket_id" {
  description = "The name (ID) of the access logs bucket."
  value       = aws_s3_bucket.logs.id
}

output "logs_bucket_arn" {
  description = "The ARN of the access logs bucket."
  value       = aws_s3_bucket.logs.arn
}

output "logs_bucket_domain_name" {
  description = "The bucket domain name of the access logs bucket."
  value       = aws_s3_bucket.logs.bucket_domain_name
}

# Build Artifacts Bucket
output "artifacts_bucket_id" {
  description = "The name (ID) of the build artifacts bucket."
  value       = aws_s3_bucket.artifacts.id
}

output "artifacts_bucket_arn" {
  description = "The ARN of the build artifacts bucket."
  value       = aws_s3_bucket.artifacts.arn
}

output "artifacts_bucket_domain_name" {
  description = "The bucket domain name of the build artifacts bucket."
  value       = aws_s3_bucket.artifacts.bucket_domain_name
}
