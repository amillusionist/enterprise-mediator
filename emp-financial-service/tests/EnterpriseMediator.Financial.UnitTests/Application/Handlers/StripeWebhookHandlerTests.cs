using EnterpriseMediator.Financial.Application.Features.Payments.EventHandlers;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using EnterpriseMediator.Financial.Domain.Interfaces;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.Financial.UnitTests.Application.Handlers;

public class StripeWebhookHandlerTests
{
    private readonly Mock<IFinancialRepository> _repositoryMock;
    private readonly Mock<ILogger<StripeWebhookHandler>> _loggerMock;
    private readonly StripeWebhookHandler _sut;

    public StripeWebhookHandlerTests()
    {
        _repositoryMock = new Mock<IFinancialRepository>();
        _loggerMock = new Mock<ILogger<StripeWebhookHandler>>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.UnitOfWork).Returns(unitOfWorkMock.Object);

        _sut = new StripeWebhookHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPayment_ShouldMarkInvoiceAsPaidAndCreateTransaction()
    {
        var invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(5000m, Currency.USD));
        invoice.SetPaymentIntent("pi_test_123");

        _repositoryMock.Setup(r => r.ExistsTransactionWithExternalIdAsync("evt_001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.GetInvoiceByPaymentIntentIdAsync("pi_test_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new ProcessStripePaymentCommand("pi_test_123", 500000, "usd", "evt_001");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        _repositoryMock.Verify(r => r.AddTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlreadyProcessed_ShouldReturnIdempotentSuccess()
    {
        _repositoryMock.Setup(r => r.ExistsTransactionWithExternalIdAsync("evt_001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new ProcessStripePaymentCommand("pi_test_123", 500000, "usd", "evt_001");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.GetInvoiceByPaymentIntentIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.ExistsTransactionWithExternalIdAsync("evt_001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.GetInvoiceByPaymentIntentIdAsync("pi_unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var command = new ProcessStripePaymentCommand("pi_unknown", 500000, "usd", "evt_001");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenInvoiceAlreadyPaid_ShouldReturnIdempotentSuccess()
    {
        var invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(5000m, Currency.USD));
        invoice.SetPaymentIntent("pi_test_123");
        invoice.MarkAsPaid("prev_ref", DateTime.UtcNow);

        _repositoryMock.Setup(r => r.ExistsTransactionWithExternalIdAsync("evt_002", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.GetInvoiceByPaymentIntentIdAsync("pi_test_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new ProcessStripePaymentCommand("pi_test_123", 500000, "usd", "evt_002");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
