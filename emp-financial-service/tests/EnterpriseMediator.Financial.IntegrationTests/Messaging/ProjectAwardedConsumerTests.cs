using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Application.Features.Invoices.Commands.GenerateInvoice;
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

public class ProjectAwardedConsumerTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    private readonly Mock<ISender> _senderMock = new();

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<ProjectAwardedConsumer>();
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
    public async Task Consume_ValidProjectAwardedEvent_SendsGenerateInvoiceCommand()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.IsAny<GenerateInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(invoiceId));

        var @event = new ProjectAwardedIntegrationEvent
        {
            ProjectId = projectId,
            VendorId = vendorId,
            ProposalId = proposalId,
            ProposedCost = 50000m,
            Currency = "USD",
            AwardedAt = DateTime.UtcNow
        };

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        (await _harness.Consumed.Any<ProjectAwardedIntegrationEvent>()).Should().BeTrue();

        var consumerHarness = _harness.GetConsumerHarness<ProjectAwardedConsumer>();
        (await consumerHarness.Consumed.Any<ProjectAwardedIntegrationEvent>()).Should().BeTrue();

        _senderMock.Verify(s => s.Send(
            It.Is<GenerateInvoiceCommand>(cmd =>
                cmd.ProjectId == projectId &&
                cmd.Amount == 50000m &&
                cmd.CurrencyCode == "USD"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_WhenGenerateInvoiceFails_DoesNotThrow()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<GenerateInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure("Client not found"));

        var @event = new ProjectAwardedIntegrationEvent
        {
            ProjectId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            ProposalId = Guid.NewGuid(),
            ProposedCost = 10000m,
            Currency = "EUR",
            AwardedAt = DateTime.UtcNow
        };

        // Act
        await _harness.Bus.Publish(@event);

        // Assert — consumer handled it without throwing (message not faulted)
        (await _harness.Consumed.Any<ProjectAwardedIntegrationEvent>()).Should().BeTrue();

        var consumerHarness = _harness.GetConsumerHarness<ProjectAwardedConsumer>();
        (await consumerHarness.Consumed.Any<ProjectAwardedIntegrationEvent>()).Should().BeTrue();

        _senderMock.Verify(s => s.Send(
            It.IsAny<GenerateInvoiceCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_MessageContractProperties_AreDeserializedCorrectly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var awardedAt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GenerateInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var @event = new ProjectAwardedIntegrationEvent
        {
            ProjectId = projectId,
            VendorId = vendorId,
            ProposalId = proposalId,
            ProposedCost = 75000.50m,
            Currency = "GBP",
            AwardedAt = awardedAt
        };

        // Act
        await _harness.Bus.Publish(@event);

        // Assert
        (await _harness.Consumed.Any<ProjectAwardedIntegrationEvent>(
            x => x.Context.Message.ProjectId == projectId &&
                 x.Context.Message.VendorId == vendorId &&
                 x.Context.Message.ProposalId == proposalId &&
                 x.Context.Message.ProposedCost == 75000.50m &&
                 x.Context.Message.Currency == "GBP" &&
                 x.Context.Message.AwardedAt == awardedAt))
            .Should().BeTrue();
    }
}
