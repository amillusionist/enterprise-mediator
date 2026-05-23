# ---------------------------------------------------------------------------------------------------------------------
# ELASTICACHE MODULE
# Provisions an ElastiCache Redis replication group with encryption, automatic failover,
# subnet group, parameter group, and security group for the EMP platform.
# ---------------------------------------------------------------------------------------------------------------------

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

locals {
  name_prefix         = "${var.project_name}-${var.environment}"
  replication_group_id = "${local.name_prefix}-redis"
  is_clustered        = var.num_cache_nodes > 1
}

# ---------------------------------------------------------------------------------------------------------------------
# ELASTICACHE SUBNET GROUP
# Places the Redis nodes in the provided private subnets across availability zones.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_elasticache_subnet_group" "this" {
  name        = "${local.name_prefix}-redis-subnet"
  description = "Subnet group for ${local.name_prefix} Redis cluster"
  subnet_ids  = var.subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-redis-subnet"
    }
  )
}

# ---------------------------------------------------------------------------------------------------------------------
# ELASTICACHE PARAMETER GROUP
# Custom parameter group for Redis 7.x family with tunable configuration.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_elasticache_parameter_group" "this" {
  name        = "${local.name_prefix}-redis7"
  family      = "redis7"
  description = "Custom parameter group for ${local.name_prefix} Redis 7.x"

  parameter {
    name  = "maxmemory-policy"
    value = var.maxmemory_policy
  }

  parameter {
    name  = "notify-keyspace-events"
    value = var.notify_keyspace_events
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-redis7"
    }
  )

  lifecycle {
    create_before_destroy = true
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# SECURITY GROUP
# Controls network access to the Redis cluster. Allows inbound on port 6379 from the
# VPC CIDR block and from explicitly allowed security groups (e.g., EKS nodes).
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_security_group" "this" {
  name        = "${local.name_prefix}-redis-sg"
  description = "Security group for ${local.name_prefix} ElastiCache Redis"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-redis-sg"
    }
  )
}

resource "aws_security_group_rule" "ingress_vpc_cidr" {
  type              = "ingress"
  from_port         = 6379
  to_port           = 6379
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr_block]
  security_group_id = aws_security_group.this.id
  description       = "Allow Redis access from VPC CIDR"
}

resource "aws_security_group_rule" "ingress_allowed_sgs" {
  count = length(var.allowed_security_groups)

  type                     = "ingress"
  from_port                = 6379
  to_port                  = 6379
  protocol                 = "tcp"
  source_security_group_id = var.allowed_security_groups[count.index]
  security_group_id        = aws_security_group.this.id
  description              = "Allow Redis access from allowed security group ${count.index}"
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
# ELASTICACHE REPLICATION GROUP
# Provisions Redis with replication, encryption at rest and in transit, automatic failover
# (when multi-node), snapshot retention, and a configurable maintenance window.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_elasticache_replication_group" "this" {
  replication_group_id = local.replication_group_id
  description          = "Redis replication group for ${local.name_prefix}"

  # Engine configuration
  engine               = "redis"
  engine_version       = var.engine_version
  node_type            = var.node_type
  port                 = 6379
  parameter_group_name = aws_elasticache_parameter_group.this.name

  # Cluster topology
  num_cache_clusters = var.num_cache_nodes
  subnet_group_name  = aws_elasticache_subnet_group.this.name
  security_group_ids = [aws_security_group.this.id]

  # High availability — automatic failover requires at least 2 nodes
  automatic_failover_enabled = local.is_clustered
  multi_az_enabled           = local.is_clustered

  # Encryption
  at_rest_encryption_enabled = true
  transit_encryption_enabled = true
  auth_token                 = var.auth_token

  # Snapshots
  snapshot_retention_limit = var.snapshot_retention_limit
  snapshot_window          = var.snapshot_window

  # Maintenance
  maintenance_window = var.maintenance_window

  # Update behavior
  apply_immediately = var.apply_immediately

  # Notifications
  notification_topic_arn = var.notification_topic_arn

  # Logging
  dynamic "log_delivery_configuration" {
    for_each = var.enable_slow_log ? [1] : []
    content {
      destination      = aws_cloudwatch_log_group.slow_log[0].name
      destination_type = "cloudwatch-logs"
      log_format       = "json"
      log_type         = "slow-log"
    }
  }

  dynamic "log_delivery_configuration" {
    for_each = var.enable_engine_log ? [1] : []
    content {
      destination      = aws_cloudwatch_log_group.engine_log[0].name
      destination_type = "cloudwatch-logs"
      log_format       = "json"
      log_type         = "engine-log"
    }
  }

  tags = merge(
    var.tags,
    {
      Name = local.replication_group_id
    }
  )

  lifecycle {
    ignore_changes = [num_cache_clusters]
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# CLOUDWATCH LOG GROUPS (conditional)
# Created only when slow-log or engine-log delivery is enabled.
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_cloudwatch_log_group" "slow_log" {
  count = var.enable_slow_log ? 1 : 0

  name              = "/aws/elasticache/${local.replication_group_id}/slow-log"
  retention_in_days = var.log_retention_days

  tags = merge(
    var.tags,
    {
      Name = "${local.replication_group_id}-slow-log"
    }
  )
}

resource "aws_cloudwatch_log_group" "engine_log" {
  count = var.enable_engine_log ? 1 : 0

  name              = "/aws/elasticache/${local.replication_group_id}/engine-log"
  retention_in_days = var.log_retention_days

  tags = merge(
    var.tags,
    {
      Name = "${local.replication_group_id}-engine-log"
    }
  )
}
