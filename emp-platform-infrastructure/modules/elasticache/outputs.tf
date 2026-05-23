# ---------------------------------------------------------------------------------------------------------------------
# ELASTICACHE MODULE — OUTPUTS
# ---------------------------------------------------------------------------------------------------------------------

output "primary_endpoint_address" {
  description = "The primary endpoint address of the Redis replication group."
  value       = aws_elasticache_replication_group.this.primary_endpoint_address
}

output "reader_endpoint_address" {
  description = "The reader endpoint address of the Redis replication group (load-balanced across read replicas)."
  value       = aws_elasticache_replication_group.this.reader_endpoint_address
}

output "port" {
  description = "The port number on which the Redis cluster accepts connections."
  value       = aws_elasticache_replication_group.this.port
}

output "security_group_id" {
  description = "The ID of the security group created for the Redis cluster."
  value       = aws_security_group.this.id
}

output "replication_group_id" {
  description = "The ID of the ElastiCache replication group."
  value       = aws_elasticache_replication_group.this.id
}

output "replication_group_arn" {
  description = "The ARN of the ElastiCache replication group."
  value       = aws_elasticache_replication_group.this.arn
}

output "subnet_group_name" {
  description = "The name of the ElastiCache subnet group."
  value       = aws_elasticache_subnet_group.this.name
}

output "parameter_group_name" {
  description = "The name of the ElastiCache parameter group."
  value       = aws_elasticache_parameter_group.this.name
}
