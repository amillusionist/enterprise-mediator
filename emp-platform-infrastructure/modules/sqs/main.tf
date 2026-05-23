################################################################################
# SQS Module — Enterprise Mediator Platform
#
# Creates three SQS queues with corresponding dead-letter queues:
#   1. SOW Upload queue
#   2. Payment Events queue
#   3. Notifications queue
#
# Each queue is KMS-encrypted, has a redrive policy to its DLQ, and a
# resource policy restricting access to the owning AWS account.
################################################################################

data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

# ------------------------------------------------------------------------------
# Local helpers
# ------------------------------------------------------------------------------
locals {
  account_id = data.aws_caller_identity.current.account_id
  region     = data.aws_region.current.name

  queue_definitions = {
    sow_upload = {
      name = var.sow_upload_queue_name
    }
    payment_events = {
      name = var.payment_events_queue_name
    }
    notifications = {
      name = var.notification_queue_name
    }
  }
}

# ------------------------------------------------------------------------------
# Dead-Letter Queues
# ------------------------------------------------------------------------------
resource "aws_sqs_queue" "dlq" {
  for_each = local.queue_definitions

  name                       = "${each.value.name}-dlq"
  visibility_timeout_seconds = var.visibility_timeout_seconds
  message_retention_seconds  = var.message_retention_seconds
  kms_master_key_id          = var.kms_key_id

  tags = merge(var.tags, {
    Name        = "${each.value.name}-dlq"
    Environment = var.environment
    Purpose     = "Dead-letter queue for ${each.value.name}"
  })
}

resource "aws_sqs_queue_policy" "dlq" {
  for_each = local.queue_definitions

  queue_url = aws_sqs_queue.dlq[each.key].url
  policy    = data.aws_iam_policy_document.queue_policy_dlq[each.key].json
}

data "aws_iam_policy_document" "queue_policy_dlq" {
  for_each = local.queue_definitions

  statement {
    sid    = "AllowAccountAccess"
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = ["arn:aws:iam::${local.account_id}:root"]
    }

    actions = [
      "sqs:SendMessage",
      "sqs:ReceiveMessage",
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:GetQueueUrl",
      "sqs:ChangeMessageVisibility",
    ]

    resources = [aws_sqs_queue.dlq[each.key].arn]
  }

  statement {
    sid    = "DenyNonAccountAccess"
    effect = "Deny"

    principals {
      type        = "AWS"
      identifiers = ["*"]
    }

    actions = ["sqs:*"]

    resources = [aws_sqs_queue.dlq[each.key].arn]

    condition {
      test     = "StringNotEquals"
      variable = "aws:PrincipalAccount"
      values   = [local.account_id]
    }
  }
}

# ------------------------------------------------------------------------------
# Primary Queues
# ------------------------------------------------------------------------------
resource "aws_sqs_queue" "main" {
  for_each = local.queue_definitions

  name                       = each.value.name
  visibility_timeout_seconds = var.visibility_timeout_seconds
  message_retention_seconds  = var.message_retention_seconds
  kms_master_key_id          = var.kms_key_id

  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.dlq[each.key].arn
    maxReceiveCount     = 3
  })

  tags = merge(var.tags, {
    Name        = each.value.name
    Environment = var.environment
  })
}

resource "aws_sqs_queue_policy" "main" {
  for_each = local.queue_definitions

  queue_url = aws_sqs_queue.main[each.key].url
  policy    = data.aws_iam_policy_document.queue_policy_main[each.key].json
}

data "aws_iam_policy_document" "queue_policy_main" {
  for_each = local.queue_definitions

  statement {
    sid    = "AllowAccountAccess"
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = ["arn:aws:iam::${local.account_id}:root"]
    }

    actions = [
      "sqs:SendMessage",
      "sqs:ReceiveMessage",
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:GetQueueUrl",
      "sqs:ChangeMessageVisibility",
    ]

    resources = [aws_sqs_queue.main[each.key].arn]
  }

  statement {
    sid    = "DenyNonAccountAccess"
    effect = "Deny"

    principals {
      type        = "AWS"
      identifiers = ["*"]
    }

    actions = ["sqs:*"]

    resources = [aws_sqs_queue.main[each.key].arn]

    condition {
      test     = "StringNotEquals"
      variable = "aws:PrincipalAccount"
      values   = [local.account_id]
    }
  }
}

# ------------------------------------------------------------------------------
# Redrive Allow Policy — permit primary queues to use their respective DLQs
# ------------------------------------------------------------------------------
resource "aws_sqs_queue_redrive_allow_policy" "dlq" {
  for_each = local.queue_definitions

  queue_url = aws_sqs_queue.dlq[each.key].url

  redrive_allow_policy = jsonencode({
    redrivePermission = "byQueue"
    sourceQueueArns   = [aws_sqs_queue.main[each.key].arn]
  })
}
