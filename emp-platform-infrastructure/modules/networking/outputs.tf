# ---------------------------------------------------------------------------------------------------------------------
# NETWORKING MODULE — OUTPUTS
# ---------------------------------------------------------------------------------------------------------------------

output "vpc_id" {
  description = "The ID of the VPC."
  value       = aws_vpc.this.id
}

output "vpc_cidr_block" {
  description = "The CIDR block of the VPC."
  value       = aws_vpc.this.cidr_block
}

output "public_subnet_ids" {
  description = "List of public subnet IDs."
  value       = aws_subnet.public[*].id
}

output "private_subnet_ids" {
  description = "List of private subnet IDs (application layer)."
  value       = aws_subnet.private[*].id
}

output "database_subnet_ids" {
  description = "List of database subnet IDs (isolated)."
  value       = aws_subnet.database[*].id
}

output "database_subnet_group_name" {
  description = "Name of the RDS DB subnet group."
  value       = aws_db_subnet_group.database.name
}

output "nat_gateway_public_ips" {
  description = "Public IP addresses of the NAT Gateways."
  value       = aws_eip.nat[*].public_ip
}

output "internet_gateway_id" {
  description = "The ID of the Internet Gateway."
  value       = aws_internet_gateway.this.id
}

output "public_route_table_id" {
  description = "The ID of the public route table."
  value       = aws_route_table.public.id
}

output "private_route_table_ids" {
  description = "List of private route table IDs."
  value       = aws_route_table.private[*].id
}
