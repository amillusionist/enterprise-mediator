using MediatR;

namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Marker interface for CQRS queries that return a <see cref="Common.Result{T}"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the value inside the Result.</typeparam>
public interface IQuery<TResponse> : IRequest<Common.Result<TResponse>>
{
}
