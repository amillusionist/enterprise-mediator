namespace EnterpriseMediator.AiWorker.Application.Interfaces;

/// <summary>
/// Abstraction over the messaging infrastructure for publishing events.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event message to the message bus.
    /// </summary>
    Task PublishAsync<T>(T eventMessage, CancellationToken token = default) where T : class;
}
