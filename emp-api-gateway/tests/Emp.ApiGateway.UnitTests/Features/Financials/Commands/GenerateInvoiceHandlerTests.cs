using Emp.ApiGateway.Application.Features.Financials.Commands;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Financials;
using EnterpriseMediator.Contracts.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Financials.Commands;

public class GenerateInvoiceHandlerTests
{
    private readonly IFinancialServiceClient _financialService = Substitute.For<IFinancialServiceClient>();
    private readonly ILogger<GenerateInvoiceHandler> _logger = Substitute.For<ILogger<GenerateInvoiceHandler>>();
    private readonly GenerateInvoiceHandler _sut;

    public GenerateInvoiceHandlerTests()
    {
        _sut = new GenerateInvoiceHandler(_financialService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldGenerateAndReturnInvoice()
    {
        var projectId = Guid.NewGuid();
        var command = new GenerateInvoiceCommand
        {
            ProjectId = projectId,
            Amount = 50_000m,
            Currency = "USD",
            DueDate = DateTimeOffset.UtcNow.AddDays(30),
            Description = "Phase 1 invoice"
        };

        var expectedInvoice = new InvoiceDto
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ClientId = Guid.NewGuid(),
            Amount = 50_000m,
            Currency = "USD",
            Status = InvoiceStatus.Draft,
            DueDate = command.DueDate,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _financialService
            .GenerateInvoiceAsync(Arg.Any<GenerateInvoiceRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedInvoice);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Amount.Should().Be(50_000m);
        result.Status.Should().Be(InvoiceStatus.Draft);
    }

    [Fact]
    public async Task Handle_ShouldMapCommandToRequest()
    {
        var projectId = Guid.NewGuid();
        var dueDate = DateTimeOffset.UtcNow.AddDays(15);
        var command = new GenerateInvoiceCommand
        {
            ProjectId = projectId,
            Amount = 10_000m,
            Currency = "EUR",
            DueDate = dueDate,
            Description = "Consulting fees"
        };

        _financialService
            .GenerateInvoiceAsync(Arg.Any<GenerateInvoiceRequest>(), Arg.Any<CancellationToken>())
            .Returns(new InvoiceDto
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                ClientId = Guid.NewGuid(),
                Amount = 10_000m,
                Currency = "EUR",
                Status = InvoiceStatus.Draft,
                CreatedAt = DateTimeOffset.UtcNow
            });

        await _sut.Handle(command, CancellationToken.None);

        await _financialService.Received(1).GenerateInvoiceAsync(
            Arg.Is<GenerateInvoiceRequest>(r =>
                r.ProjectId == projectId &&
                r.Amount == 10_000m &&
                r.Currency == "EUR" &&
                r.DueDate == dueDate &&
                r.Description == "Consulting fees"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagate()
    {
        var command = new GenerateInvoiceCommand
        {
            ProjectId = Guid.NewGuid(),
            Amount = 1_000m,
            Currency = "USD"
        };

        _financialService
            .GenerateInvoiceAsync(Arg.Any<GenerateInvoiceRequest>(), Arg.Any<CancellationToken>())
            .Returns<InvoiceDto>(x => throw new HttpRequestException("Financial service down"));

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Financial service down");
    }
}
