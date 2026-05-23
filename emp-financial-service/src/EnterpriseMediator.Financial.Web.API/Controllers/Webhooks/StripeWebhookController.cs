using EnterpriseMediator.Financial.Application.Features.Payments.EventHandlers;
using EnterpriseMediator.Financial.Infrastructure.Persistence.Configurations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace EnterpriseMediator.Financial.Web.API.Controllers.Webhooks;

[ApiController]
[Route("api/webhooks/stripe")]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private readonly ISender _sender;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        ISender sender,
        IOptions<StripeSettings> stripeSettings,
        ILogger<StripeWebhookController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _stripeSettings = stripeSettings?.Value ?? throw new ArgumentNullException(nameof(stripeSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles incoming webhooks from Stripe.
    /// </summary>
    /// <response code="200">Event acknowledged.</response>
    /// <response code="400">Invalid signature.</response>
    /// <response code="500">Processing error (Stripe will retry).</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        string json;
        try
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            json = await reader.ReadToEndAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read Stripe webhook body");
            return BadRequest("Could not read request body.");
        }

        if (!Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader))
        {
            _logger.LogWarning("Stripe webhook missing signature header");
            return BadRequest("Missing Stripe-Signature header.");
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                _stripeSettings.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe signature verification failed");
            return BadRequest("Invalid Stripe signature.");
        }

        _logger.LogInformation("Received Stripe Webhook: {EventType} | ID: {EventId}",
            stripeEvent.Type, stripeEvent.Id);

        try
        {
            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
                    {
                        _logger.LogInformation("Processing PaymentIntent Succeeded: {PaymentIntentId}",
                            paymentIntent.Id);

                        var command = new ProcessStripePaymentCommand(
                            paymentIntent.Id,
                            paymentIntent.Amount,
                            paymentIntent.Currency,
                            stripeEvent.Id);

                        var result = await _sender.Send(command, cancellationToken);

                        if (result.IsFailure)
                        {
                            _logger.LogWarning("Payment processing failed: {Error}", result.Error);
                            // Return 200 to acknowledge receipt; failure logged for investigation
                        }
                    }
                    break;

                case Events.PaymentIntentPaymentFailed:
                    if (stripeEvent.Data.Object is PaymentIntent failedIntent)
                    {
                        _logger.LogWarning("PaymentIntent Failed: {PaymentIntentId} | Reason: {FailureReason}",
                            failedIntent.Id, failedIntent.LastPaymentError?.Message);
                    }
                    break;

                default:
                    _logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook event {EventId}", stripeEvent.Id);
            return StatusCode(500, "Error processing event.");
        }
    }
}
