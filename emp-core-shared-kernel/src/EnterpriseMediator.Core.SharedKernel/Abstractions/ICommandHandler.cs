using MediatR;

namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Handler for a command that returns a non-generic Result.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Common.Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handler for a command that returns a Result&lt;TResponse&gt;.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Common.Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
