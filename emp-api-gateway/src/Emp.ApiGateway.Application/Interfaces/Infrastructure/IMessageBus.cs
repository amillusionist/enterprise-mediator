namespace Emp.ApiGateway.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Abstraction for publishing messages to a message broker.
    /// Implemented by MassTransitPublisher in the Infrastructure layer.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Publishes a message to the message broker.
        /// </summary>
        /// <typeparam name="T">The type of message to publish.</typeparam>
        /// <param name="message">The message payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}
