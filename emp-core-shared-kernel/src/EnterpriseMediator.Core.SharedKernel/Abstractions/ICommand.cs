using MediatR;

namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Marker interface for CQRS commands that return a <see cref="Common.Result"/>.
/// </summary>
public interface ICommand : IRequest<Common.Result>
{
}

/// <summary>
/// Marker interface for CQRS commands that return a <see cref="Common.Result{T}"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the value inside the Result.</typeparam>
public interface ICommand<TResponse> : IRequest<Common.Result<TResponse>>
{
}
