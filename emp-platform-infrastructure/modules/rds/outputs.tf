# ---------------------------------------------------------------------------------------------------------------------
# RDS MODULE — OUTPUTS
# ---------------------------------------------------------------------------------------------------------------------

output "endpoint" {
  description = "The connection endpoint of the RDS instance (includes port, e.g., host:5432)."
  value       = aws_db_instance.this.endpoint
}

output "address" {
  description = "The hostname of the RDS instance (without port)."
  value       = aws_db_instance.this.address
}

output "port" {
  description = "The port on which the RDS instance accepts connections."
  value       = aws_db_instance.this.port
}

output "db_name" {
  description = "The name of the default database."
  value       = aws_db_instance.this.db_name
}

output "instance_id" {
  description = "The RDS instance identifier."
  value       = aws_db_instance.this.id
}

output "security_group_id" {
  description = "The security group ID attached to the RDS instance."
  value       = aws_security_group.this.id
}
