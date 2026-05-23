using MediatR;

namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Handler for a query that returns a Result&lt;TResponse&gt;.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Common.Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
