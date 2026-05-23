# ---------------------------------------------------------------------------------------------------------------------
# S3 MODULE — VARIABLES
# Input variables for provisioning SOW documents, access logs, and build artifacts buckets.
# ---------------------------------------------------------------------------------------------------------------------

variable "project_name" {
  description = "Project name used for resource naming and tagging."
  type        = string
}

variable "environment" {
  description = "Deployment environment (e.g., dev, staging, prod)."
  type        = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "kms_key_arn" {
  description = "ARN of the KMS Customer Managed Key used for SSE-KMS encryption on all buckets."
  type        = string
}

variable "sow_bucket_name" {
  description = "Name for the SOW documents bucket."
  type        = string
}

variable "logs_bucket_name" {
  description = "Name for the access logs bucket."
  type        = string
}

variable "artifacts_bucket_name" {
  description = "Name for the build artifacts bucket."
  type        = string
}

variable "force_destroy" {
  description = "Whether to allow Terraform to destroy non-empty buckets. Should be false in production."
  type        = bool
  default     = false
}

variable "tags" {
  description = "Common tags applied to all resources in this module."
  type        = map(string)
  default     = {}
}

variable "cors_allowed_origins" {
  description = "List of allowed origins for CORS on the SOW documents bucket. Defaults to wildcard for dev; override in prod."
  type        = list(string)
  default     = ["*"]
}

variable "sow_ia_transition_days" {
  description = "Number of days before SOW objects transition to Infrequent Access storage class."
  type        = number
  default     = 90
}

variable "sow_glacier_transition_days" {
  description = "Number of days before SOW objects transition to Glacier storage class."
  type        = number
  default     = 365
}

variable "logs_expiration_days" {
  description = "Number of days before access log objects expire and are deleted."
  type        = number
  default     = 90
}
