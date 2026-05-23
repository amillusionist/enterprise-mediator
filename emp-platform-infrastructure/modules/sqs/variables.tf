################################################################################
# SQS Module — Variables
################################################################################

variable "project_name" {
  description = "Project name used for resource naming and tagging."
  type        = string
}

variable "environment" {
  description = "Deployment environment (e.g. dev, staging, prod)."
  type        = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be one of: dev, staging, prod."
  }
}

variable "sow_upload_queue_name" {
  description = "Name of the SOW upload SQS queue."
  type        = string
}

variable "payment_events_queue_name" {
  description = "Name of the payment events SQS queue."
  type        = string
}

variable "notification_queue_name" {
  description = "Name of the notifications SQS queue."
  type        = string
}

variable "kms_key_id" {
  description = "ARN of the KMS key used for server-side encryption of SQS messages."
  type        = string
}

variable "visibility_timeout_seconds" {
  description = "The visibility timeout for the queues, in seconds. Should be at least 6x the consumer processing time."
  type        = number
  default     = 300

  validation {
    condition     = var.visibility_timeout_seconds >= 0 && var.visibility_timeout_seconds <= 43200
    error_message = "visibility_timeout_seconds must be between 0 and 43200 (12 hours)."
  }
}

variable "message_retention_seconds" {
  description = "The number of seconds SQS retains a message. Must be between 60 and 1209600 (14 days)."
  type        = number
  default     = 345600

  validation {
    condition     = var.message_retention_seconds >= 60 && var.message_retention_seconds <= 1209600
    error_message = "message_retention_seconds must be between 60 and 1209600 (14 days)."
  }
}

variable "tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
