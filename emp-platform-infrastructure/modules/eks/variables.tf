# ---------------------------------------------------------------------------------------------------------------------
# EKS MODULE — VARIABLES
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
  description = "ID of the VPC where the EKS cluster will be deployed."
  type        = string
}

variable "subnet_ids" {
  description = "List of subnet IDs for EKS cluster and node groups (private subnets)."
  type        = list(string)
}

variable "cluster_version" {
  description = "Kubernetes version for the EKS cluster."
  type        = string
  default     = "1.29"
}

variable "node_groups" {
  description = "Map of EKS managed node group configurations."
  type = map(object({
    desired_size   = number
    min_size       = number
    max_size       = number
    instance_types = optional(list(string), ["t3.medium"])
    capacity_type  = optional(string, "ON_DEMAND")
    ami_type       = optional(string, "AL2_x86_64")
    disk_size      = optional(number, 50)
    labels         = optional(map(string), {})
  }))
}

variable "kms_key_arn" {
  description = "KMS key ARN for encrypting EKS secrets. Set to null to disable."
  type        = string
  default     = null
}

variable "cluster_endpoint_public_access" {
  description = "Whether the EKS cluster API endpoint is publicly accessible."
  type        = bool
  default     = true
}

variable "cluster_endpoint_private_access" {
  description = "Whether the EKS cluster API endpoint is accessible within the VPC."
  type        = bool
  default     = true
}

variable "cluster_endpoint_public_access_cidrs" {
  description = "List of CIDR blocks allowed to access the public EKS API endpoint."
  type        = list(string)
  default     = ["0.0.0.0/0"]
}

variable "cluster_log_types" {
  description = "List of EKS cluster log types to enable (api, audit, authenticator, controllerManager, scheduler)."
  type        = list(string)
  default     = ["api", "audit", "authenticator"]
}

variable "tags" {
  description = "Common tags to apply to all resources."
  type        = map(string)
  default     = {}
}
