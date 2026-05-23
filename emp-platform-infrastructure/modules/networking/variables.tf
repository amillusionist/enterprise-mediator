# ---------------------------------------------------------------------------------------------------------------------
# NETWORKING MODULE — VARIABLES
# ---------------------------------------------------------------------------------------------------------------------

variable "project_name" {
  description = "Name of the project, used in resource naming and tagging."
  type        = string
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)."
  type        = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC."
  type        = string
  default     = "10.0.0.0/16"

  validation {
    condition     = can(cidrhost(var.vpc_cidr, 0))
    error_message = "vpc_cidr must be a valid CIDR block."
  }
}

variable "availability_zones" {
  description = "List of AWS availability zones to deploy subnets into."
  type        = list(string)
}

variable "public_subnet_cidrs" {
  description = "CIDR blocks for public subnets (one per AZ). Must match the length of availability_zones."
  type        = list(string)
}

variable "private_subnet_cidrs" {
  description = "CIDR blocks for private subnets (one per AZ). Must match the length of availability_zones."
  type        = list(string)
}

variable "database_subnet_cidrs" {
  description = "CIDR blocks for database subnets (one per AZ). Must match the length of availability_zones."
  type        = list(string)
}

variable "enable_nat_gateway" {
  description = "Whether to create NAT Gateway(s) for private subnet internet access."
  type        = bool
  default     = true
}

variable "single_nat_gateway" {
  description = "Use a single NAT Gateway (cost saving for non-prod). If false, one NAT per AZ for HA."
  type        = bool
  default     = true
}

variable "enable_dns_hostnames" {
  description = "Enable DNS hostnames in the VPC (required for EKS)."
  type        = bool
  default     = true
}

variable "enable_dns_support" {
  description = "Enable DNS support in the VPC."
  type        = bool
  default     = true
}

variable "enable_flow_logs" {
  description = "Enable VPC flow logs for security auditing."
  type        = bool
  default     = true
}

variable "flow_log_retention_days" {
  description = "Number of days to retain VPC flow logs in CloudWatch."
  type        = number
  default     = 30
}

variable "kms_key_arn" {
  description = "KMS key ARN for encrypting flow logs. Optional."
  type        = string
  default     = null
}

variable "tags" {
  description = "Common tags to apply to all resources."
  type        = map(string)
  default     = {}
}
