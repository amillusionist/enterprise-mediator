using System.Net;

namespace EnterpriseMediator.Core.SharedKernel.Exceptions;

/// <summary>
/// Exception thrown when an operation conflicts with current resource state (e.g., duplicate, concurrency).
/// Maps to HTTP 409 Conflict.
/// </summary>
public class ConflictException : CustomException
{
    public ConflictException(string message)
        : base(message, statusCode: HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string name, object key)
        : base($"Entity \"{name}\" ({key}) already exists.", statusCode: HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.Conflict)
    {
    }
}
