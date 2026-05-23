using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ConfigureFinancials;

public record ConfigureFinancialsCommand(
    Guid ProjectId,
    decimal? FixedMargin,
    decimal? PercentageMargin) : IRequest;
