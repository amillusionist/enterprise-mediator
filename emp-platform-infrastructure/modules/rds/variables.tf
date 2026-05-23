# ---------------------------------------------------------------------------------------------------------------------
# RDS MODULE — VARIABLES
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
  description = "ID of the VPC where the RDS instance will be deployed."
  type        = string
}

variable "subnet_ids" {
  description = "List of subnet IDs for the DB subnet group (database-tier subnets)."
  type        = list(string)
}

variable "vpc_cidr_block" {
  description = "CIDR block of the VPC, used for security group ingress rules."
  type        = string
}

variable "engine" {
  description = "Database engine type."
  type        = string
  default     = "postgres"
}

variable "engine_version" {
  description = "Database engine version."
  type        = string
  default     = "16.1"
}

variable "instance_class" {
  description = "RDS instance class (e.g., db.r6g.large)."
  type        = string
  default     = "db.r6g.large"
}

variable "allocated_storage" {
  description = "Allocated storage in GiB for the RDS instance."
  type        = number
  default     = 100
}

variable "max_allocated_storage" {
  description = "Upper limit in GiB for RDS storage autoscaling. Set to 0 to disable autoscaling."
  type        = number
  default     = 500
}

variable "db_name" {
  description = "Name of the default database to create."
  type        = string
}

variable "username" {
  description = "Master username for the RDS instance."
  type        = string
  sensitive   = true
}

variable "password" {
  description = "Master password for the RDS instance. Injected via TF_VAR or Secrets Manager."
  type        = string
  sensitive   = true
}

variable "port" {
  description = "Port on which the database accepts connections."
  type        = number
  default     = 5432
}

variable "multi_az" {
  description = "Enable Multi-AZ deployment for high availability."
  type        = bool
  default     = false
}

variable "kms_key_arn" {
  description = "ARN of the KMS key for encrypting storage at rest."
  type        = string
  default     = null
}

variable "storage_encrypted" {
  description = "Whether to encrypt the RDS storage at rest."
  type        = bool
  default     = true
}

variable "deletion_protection" {
  description = "Enable deletion protection on the RDS instance."
  type        = bool
  default     = false
}

variable "allowed_security_groups" {
  description = "List of security group IDs allowed to access the database on the configured port."
  type        = list(string)
  default     = []
}

variable "force_destroy" {
  description = "If true, skip final snapshot on deletion. Use only in non-production environments."
  type        = bool
  default     = false
}

variable "backup_retention_period" {
  description = "Number of days to retain automated backups."
  type        = number
  default     = 7
}

variable "backup_window" {
  description = "Preferred UTC window for automated backups (hh24:mi-hh24:mi)."
  type        = string
  default     = "03:00-04:00"
}

variable "maintenance_window" {
  description = "Preferred UTC window for maintenance operations (ddd:hh24:mi-ddd:hh24:mi)."
  type        = string
  default     = "sun:05:00-sun:06:00"
}

variable "performance_insights_retention_period" {
  description = "Number of days to retain Performance Insights data (7 free tier, 731 for long-term)."
  type        = number
  default     = 7
}

variable "monitoring_interval" {
  description = "Enhanced Monitoring interval in seconds (0, 1, 5, 10, 15, 30, 60)."
  type        = number
  default     = 60
}

variable "tags" {
  description = "Common tags to apply to all resources."
  type        = map(string)
  default     = {}
}
