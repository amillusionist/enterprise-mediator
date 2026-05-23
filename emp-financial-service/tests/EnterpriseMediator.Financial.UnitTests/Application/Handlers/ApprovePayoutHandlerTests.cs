using EnterpriseMediator.Financial.Application.Features.Payouts.Commands.ApprovePayout;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Interfaces;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.Financial.UnitTests.Application.Handlers;

public class ApprovePayoutHandlerTests
{
    private readonly Mock<IFinancialRepository> _repositoryMock;
    private readonly Mock<IPayoutGateway> _payoutGatewayMock;
    private readonly Mock<ILogger<ApprovePayoutHandler>> _loggerMock;
    private readonly ApprovePayoutHandler _sut;

    public ApprovePayoutHandlerTests()
    {
        _repositoryMock = new Mock<IFinancialRepository>();
        _payoutGatewayMock = new Mock<IPayoutGateway>();
        _loggerMock = new Mock<ILogger<ApprovePayoutHandler>>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.UnitOfWork).Returns(unitOfWorkMock.Object);

        _sut = new ApprovePayoutHandler(_repositoryMock.Object, _payoutGatewayMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPayout_ShouldApproveAndSubmit()
    {
        var payout = Payout.Initiate(Guid.NewGuid(), Guid.NewGuid(), new Money(3000m, Currency.USD));
        _repositoryMock.Setup(r => r.GetPayoutByIdAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _payoutGatewayMock.Setup(g => g.ExecutePayoutAsync(payout, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PayoutResult { TransferId = "wise_123", Status = "processing", FeeAmount = 5.50m, FeeCurrency = "USD" });

        var command = new ApprovePayoutCommand(payout.Id, Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payout.Status.Should().Be(PayoutStatus.Processing);
        _repositoryMock.Verify(r => r.AddTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPayoutNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetPayoutByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payout?)null);

        var command = new ApprovePayoutCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}
