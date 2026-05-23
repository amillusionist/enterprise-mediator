using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Application.Features.Payouts.Commands.InitiatePayout;
using EnterpriseMediator.Financial.Application.IntegrationEvents;
using EnterpriseMediator.Financial.Infrastructure.Messaging.Consumers;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.Financial.IntegrationTests.Messaging;

public class MilestoneApprovedConsumerTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    private readonly Mock<ISender> _senderMock = new();

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<MilestoneApprovedConsumer>();
            })
            .AddSingleton(_senderMock.Object)
            .AddLogging(lb => lb.AddDebug())
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Consume_ValidMilestoneApprovedEvent_SendsInitiatePayoutCommand()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var payoutId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.IsAny<InitiatePayoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(payoutId));

        var @event = new MilestoneApprovedIntegrationEvent
        {
            ProjectId = projectId,
            MilestoneId = milestoneId,
            VendorId = vendorId,
            PayoutAmount = 15000m,
            Currency = "USD"
        };

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        (await _harness.Consumed.Any<MilestoneApprovedIntegrationEvent>()).Should().BeTrue();

        var consumerHarness = _harness.GetConsumerHarness<MilestoneApprovedConsumer>();
        (await consumerHarness.Consumed.Any<MilestoneApprovedIntegrationEvent>()).Should().BeTrue();

        _senderMock.Verify(s => s.Send(
            It.Is<InitiatePayoutCommand>(cmd =>
                cmd.VendorId == vendorId &&
                cmd.ProjectId == projectId &&
                cmd.Amount == 15000m &&
                cmd.CurrencyCode == "USD"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_WhenPayoutInitiationFails_DoesNotThrow()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<InitiatePayoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure("Vendor bank details not configured"));

        var @event = new MilestoneApprovedIntegrationEvent
        {
            ProjectId = Guid.NewGuid(),
            MilestoneId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            PayoutAmount = 5000m,
            Currency = "EUR"
        };

        // Act
        await _harness.Bus.Publish(@event);

        // Assert — consumer handled gracefully (logged warning, no fault)
        (await _harness.Consumed.Any<MilestoneApprovedIntegrationEvent>()).Should().BeTrue();

        var consumerHarness = _harness.GetConsumerHarness<MilestoneApprovedConsumer>();
        (await consumerHarness.Consumed.Any<MilestoneApprovedIntegrationEvent>()).Should().BeTrue();

        _senderMock.Verify(s => s.Send(
            It.IsAny<InitiatePayoutCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_MessageContractProperties_AreDeserializedCorrectly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.IsAny<InitiatePayoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var @event = new MilestoneApprovedIntegrationEvent
        {
            ProjectId = projectId,
            MilestoneId = milestoneId,
            VendorId = vendorId,
            PayoutAmount = 25000.75m,
            Currency = "GBP"
        };

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        (await _harness.Consumed.Any<MilestoneApprovedIntegrationEvent>(
            x => x.Context.Message.ProjectId == projectId &&
                 x.Context.Message.MilestoneId == milestoneId &&
                 x.Context.Message.VendorId == vendorId &&
                 x.Context.Message.PayoutAmount == 25000.75m &&
                 x.Context.Message.Currency == "GBP"))
            .Should().BeTrue();
    }

    [Fact]
    public async Task Consume_MapsCorrectVendorIdToPayoutCommand()
    {
        // Arrange
        var vendorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.IsAny<InitiatePayoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var @event = new MilestoneApprovedIntegrationEvent
        {
            ProjectId = projectId,
            MilestoneId = Guid.NewGuid(),
            VendorId = vendorId,
            PayoutAmount = 10000m,
            Currency = "USD"
        };

        // Act
        await _harness.Bus.Publish(@event);

        // Assert — verify VendorId is correctly passed through
        await Task.Delay(500); // Allow consumer to process
        _senderMock.Verify(s => s.Send(
            It.Is<InitiatePayoutCommand>(cmd => cmd.VendorId == vendorId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
