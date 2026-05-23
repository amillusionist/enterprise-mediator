# ---------------------------------------------------------------------------------------------------------------------
# ELASTICACHE MODULE — VARIABLES
# ---------------------------------------------------------------------------------------------------------------------

variable "project_name" {
  description = "Name of the project, used in resource naming and tagging."
  type        = string
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)."
  type        = string
}

variable "vpc_id" {
  description = "ID of the VPC where the ElastiCache cluster will be deployed."
  type        = string
}

variable "subnet_ids" {
  description = "List of subnet IDs for the ElastiCache subnet group (private subnets)."
  type        = list(string)
}

variable "vpc_cidr_block" {
  description = "CIDR block of the VPC, used to allow inbound Redis access from within the VPC."
  type        = string
}

variable "node_type" {
  description = "ElastiCache node instance type (e.g., cache.t3.micro, cache.r6g.large)."
  type        = string
  default     = "cache.t3.micro"
}

variable "num_cache_nodes" {
  description = "Number of cache nodes in the replication group. Set to 2+ for automatic failover."
  type        = number
  default     = 1
}

variable "engine_version" {
  description = "Redis engine version."
  type        = string
  default     = "7.1"
}

variable "allowed_security_groups" {
  description = "List of security group IDs allowed to access the Redis cluster on port 6379."
  type        = list(string)
  default     = []
}

variable "auth_token" {
  description = "Auth token (password) for Redis AUTH. Required when transit encryption is enabled. Set to null to disable AUTH."
  type        = string
  default     = null
  sensitive   = true
}

variable "maxmemory_policy" {
  description = "Redis maxmemory eviction policy."
  type        = string
  default     = "volatile-lru"
}

variable "notify_keyspace_events" {
  description = "Redis keyspace notifications configuration string. Empty string disables notifications."
  type        = string
  default     = ""
}

variable "snapshot_retention_limit" {
  description = "Number of days to retain automatic Redis snapshots. Set to 0 to disable."
  type        = number
  default     = 7
}

variable "snapshot_window" {
  description = "Daily time range (UTC) during which ElastiCache takes a snapshot (e.g., 03:00-05:00)."
  type        = string
  default     = "03:00-05:00"
}

variable "maintenance_window" {
  description = "Weekly time range (UTC) during which maintenance can occur (e.g., sun:05:00-sun:07:00)."
  type        = string
  default     = "sun:05:00-sun:07:00"
}

variable "apply_immediately" {
  description = "Whether changes should be applied immediately or during the next maintenance window."
  type        = bool
  default     = false
}

variable "notification_topic_arn" {
  description = "ARN of an SNS topic for ElastiCache event notifications. Set to null to disable."
  type        = string
  default     = null
}

variable "enable_slow_log" {
  description = "Whether to enable Redis slow-log delivery to CloudWatch Logs."
  type        = bool
  default     = false
}

variable "enable_engine_log" {
  description = "Whether to enable Redis engine-log delivery to CloudWatch Logs."
  type        = bool
  default     = false
}

variable "log_retention_days" {
  description = "Number of days to retain CloudWatch log groups for Redis logs."
  type        = number
  default     = 30
}

variable "tags" {
  description = "Common tags to apply to all resources."
  type        = map(string)
  default     = {}
}
