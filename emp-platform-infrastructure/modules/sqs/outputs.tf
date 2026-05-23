################################################################################
# SQS Module — Outputs
################################################################################

# ------------------------------------------------------------------------------
# SOW Upload Queue
# ------------------------------------------------------------------------------
output "sow_upload_queue_arn" {
  description = "ARN of the SOW upload SQS queue."
  value       = aws_sqs_queue.main["sow_upload"].arn
}

output "sow_upload_queue_url" {
  description = "URL of the SOW upload SQS queue."
  value       = aws_sqs_queue.main["sow_upload"].url
}

output "sow_upload_dlq_arn" {
  description = "ARN of the SOW upload dead-letter queue."
  value       = aws_sqs_queue.dlq["sow_upload"].arn
}

# ------------------------------------------------------------------------------
# Payment Events Queue
# ------------------------------------------------------------------------------
output "payment_events_queue_arn" {
  description = "ARN of the payment events SQS queue."
  value       = aws_sqs_queue.main["payment_events"].arn
}

output "payment_events_queue_url" {
  description = "URL of the payment events SQS queue."
  value       = aws_sqs_queue.main["payment_events"].url
}

output "payment_events_dlq_arn" {
  description = "ARN of the payment events dead-letter queue."
  value       = aws_sqs_queue.dlq["payment_events"].arn
}

# ------------------------------------------------------------------------------
# Notifications Queue
# ------------------------------------------------------------------------------
output "notification_queue_arn" {
  description = "ARN of the notifications SQS queue."
  value       = aws_sqs_queue.main["notifications"].arn
}

output "notification_queue_url" {
  description = "URL of the notifications SQS queue."
  value       = aws_sqs_queue.main["notifications"].url
}

output "notification_dlq_arn" {
  description = "ARN of the notifications dead-letter queue."
  value       = aws_sqs_queue.dlq["notifications"].arn
}

# ------------------------------------------------------------------------------
# Aggregate outputs (useful for IAM policies and cross-module references)
# ------------------------------------------------------------------------------
output "all_queue_arns" {
  description = "List of all primary queue ARNs."
  value = [
    aws_sqs_queue.main["sow_upload"].arn,
    aws_sqs_queue.main["payment_events"].arn,
    aws_sqs_queue.main["notifications"].arn,
  ]
}

output "all_dlq_arns" {
  description = "List of all dead-letter queue ARNs."
  value = [
    aws_sqs_queue.dlq["sow_upload"].arn,
    aws_sqs_queue.dlq["payment_events"].arn,
    aws_sqs_queue.dlq["notifications"].arn,
  ]
}
