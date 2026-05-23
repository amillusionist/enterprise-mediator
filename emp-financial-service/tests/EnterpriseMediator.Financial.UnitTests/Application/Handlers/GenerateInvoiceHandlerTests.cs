using EnterpriseMediator.Financial.Application.Features.Invoices.Commands.GenerateInvoice;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Interfaces;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.Financial.UnitTests.Application.Handlers;

public class GenerateInvoiceHandlerTests
{
    private readonly Mock<IFinancialRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<ILogger<GenerateInvoiceHandler>> _loggerMock;
    private readonly GenerateInvoiceHandler _sut;

    public GenerateInvoiceHandlerTests()
    {
        _repositoryMock = new Mock<IFinancialRepository>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _loggerMock = new Mock<ILogger<GenerateInvoiceHandler>>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.UnitOfWork).Returns(unitOfWorkMock.Object);

        _sut = new GenerateInvoiceHandler(_repositoryMock.Object, _gatewayMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithInvoiceId()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), 5000m, "USD");
        _repositoryMock.Setup(r => r.GetInvoiceByProjectIdAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);
        _gatewayMock.Setup(g => g.CreatePaymentLinkAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentLinkResult { PaymentId = "pi_test_123", PaymentUrl = "https://stripe.com/pay", Status = "open", ExpiresAt = DateTimeOffset.UtcNow.AddHours(24) });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvoiceAlreadyExists_ShouldReturnFailure()
    {
        var projectId = Guid.NewGuid();
        var existingInvoice = Invoice.Create(projectId, Guid.NewGuid(), new Money(1000m, Currency.USD));
        var command = new GenerateInvoiceCommand(projectId, Guid.NewGuid(), 5000m, "USD");

        _repositoryMock.Setup(r => r.GetInvoiceByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvoice);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        _gatewayMock.Verify(g => g.CreatePaymentLinkAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenGatewayReturnsEmptyId_ShouldReturnFailure()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), 5000m, "USD");
        _repositoryMock.Setup(r => r.GetInvoiceByProjectIdAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);
        _gatewayMock.Setup(g => g.CreatePaymentLinkAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentLinkResult { PaymentId = "", PaymentUrl = "", Status = "", ExpiresAt = DateTimeOffset.UtcNow });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("payment link");
    }

    [Fact]
    public async Task Handle_WhenGatewayThrows_ShouldReturnFailure()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), 5000m, "USD");
        _repositoryMock.Setup(r => r.GetInvoiceByProjectIdAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);
        _gatewayMock.Setup(g => g.CreatePaymentLinkAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Stripe error"));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unexpected error");
    }
}
