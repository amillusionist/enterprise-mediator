# ---------------------------------------------------------------------------------------------------------------------
# RDS MODULE
# Provisions a PostgreSQL 16 RDS instance with pgvector support, encryption at rest,
# automated backups, Performance Insights, Enhanced Monitoring, and CloudWatch log exports.
# ---------------------------------------------------------------------------------------------------------------------

data "aws_caller_identity" "current" {}
data "aws_region" "current" {}

locals {
  identifier        = "${var.project_name}-${var.environment}-postgres"
  parameter_group   = "${var.project_name}-${var.environment}-pg16"
  subnet_group      = "${var.project_name}-${var.environment}-db-subnet"
  security_group    = "${var.project_name}-${var.environment}-rds-sg"
  monitoring_role   = "${var.project_name}-${var.environment}-rds-monitoring-role"
  final_snapshot_id = "${var.project_name}-${var.environment}-final-snapshot-${formatdate("YYYYMMDDhhmmss", timestamp())}"
}

# ---------------------------------------------------------------------------------------------------------------------
# DB SUBNET GROUP
# Places the RDS instance in the dedicated database subnets.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_db_subnet_group" "this" {
  name        = local.subnet_group
  description = "Database subnet group for ${var.project_name} ${var.environment}"
  subnet_ids  = var.subnet_ids

  tags = merge(
    var.tags,
    {
      Name = local.subnet_group
    }
  )
}

# ---------------------------------------------------------------------------------------------------------------------
# SECURITY GROUP
# Allows inbound PostgreSQL traffic from the VPC CIDR and specified security groups.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_security_group" "this" {
  name        = local.security_group
  description = "Security group for RDS PostgreSQL instance"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = local.security_group
    }
  )
}

resource "aws_security_group_rule" "ingress_vpc_cidr" {
  type              = "ingress"
  from_port         = var.port
  to_port           = var.port
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr_block]
  security_group_id = aws_security_group.this.id
  description       = "Allow PostgreSQL access from VPC CIDR"
}

resource "aws_security_group_rule" "ingress_allowed_sgs" {
  count = length(var.allowed_security_groups)

  type                     = "ingress"
  from_port                = var.port
  to_port                  = var.port
  protocol                 = "tcp"
  source_security_group_id = var.allowed_security_groups[count.index]
  security_group_id        = aws_security_group.this.id
  description              = "Allow PostgreSQL access from allowed security group ${var.allowed_security_groups[count.index]}"
}

resource "aws_security_group_rule" "egress" {
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.this.id
  description       = "Allow all outbound traffic"
}

# ---------------------------------------------------------------------------------------------------------------------
# CUSTOM PARAMETER GROUP
# Enables pgvector extension and pg_stat_statements for query performance analysis.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_db_parameter_group" "this" {
  name        = local.parameter_group
  family      = "postgres16"
  description = "Custom parameter group for ${var.project_name} ${var.environment} PostgreSQL 16 with pgvector"

  parameter {
    name  = "shared_preload_libraries"
    value = "pg_stat_statements"
  }

  parameter {
    name  = "rds.allowed_extensions"
    value = "vector,pg_stat_statements,pgcrypto"
  }

  parameter {
    name  = "log_min_duration_statement"
    value = "1000"
  }

  parameter {
    name  = "log_connections"
    value = "1"
  }

  parameter {
    name  = "log_disconnections"
    value = "1"
  }

  parameter {
    name  = "log_lock_waits"
    value = "1"
  }

  parameter {
    name         = "pg_stat_statements.track"
    value        = "all"
    apply_method = "pending-reboot"
  }

  tags = merge(
    var.tags,
    {
      Name = local.parameter_group
    }
  )

  lifecycle {
    create_before_destroy = true
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# IAM ROLE FOR ENHANCED MONITORING
# Required for RDS Enhanced Monitoring to publish metrics to CloudWatch.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_iam_role" "monitoring" {
  name = local.monitoring_role

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "monitoring.rds.amazonaws.com"
        }
      }
    ]
  })

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "monitoring" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
  role       = aws_iam_role.monitoring.name
}

# ---------------------------------------------------------------------------------------------------------------------
# RDS POSTGRESQL INSTANCE
# Production-ready PostgreSQL 16 instance with encryption, backups, monitoring, and pgvector support.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_db_instance" "this" {
  identifier = local.identifier

  # Engine configuration
  engine               = var.engine
  engine_version       = var.engine_version
  instance_class       = var.instance_class
  parameter_group_name = aws_db_parameter_group.this.name

  # Storage
  allocated_storage     = var.allocated_storage
  max_allocated_storage = var.max_allocated_storage > 0 ? var.max_allocated_storage : null
  storage_type          = "gp3"
  storage_encrypted     = var.storage_encrypted
  kms_key_id            = var.kms_key_arn

  # Database credentials
  db_name  = var.db_name
  username = var.username
  password = var.password
  port     = var.port

  # Networking
  db_subnet_group_name   = aws_db_subnet_group.this.name
  vpc_security_group_ids = [aws_security_group.this.id]
  publicly_accessible    = false
  multi_az               = var.multi_az

  # Backup configuration
  backup_retention_period = var.backup_retention_period
  backup_window           = var.backup_window
  maintenance_window      = var.maintenance_window
  copy_tags_to_snapshot   = true

  # Final snapshot
  skip_final_snapshot       = var.force_destroy
  final_snapshot_identifier = var.force_destroy ? null : "${var.project_name}-${var.environment}-final-snapshot"
  deletion_protection       = var.deletion_protection

  # Performance Insights
  performance_insights_enabled          = true
  performance_insights_retention_period = var.performance_insights_retention_period
  performance_insights_kms_key_id       = var.kms_key_arn

  # Enhanced Monitoring
  monitoring_interval = var.monitoring_interval
  monitoring_role_arn = aws_iam_role.monitoring.arn

  # CloudWatch Log Exports
  enabled_cloudwatch_logs_exports = ["postgresql", "upgrade"]

  # Upgrades
  auto_minor_version_upgrade  = true
  allow_major_version_upgrade = false

  # Apply changes during maintenance window to avoid disruption
  apply_immediately = var.environment != "prod"

  tags = merge(
    var.tags,
    {
      Name = local.identifier
    }
  )

  depends_on = [
    aws_iam_role_policy_attachment.monitoring,
  ]

  lifecycle {
    ignore_changes = [
      password,
      final_snapshot_identifier,
    ]
  }
}
