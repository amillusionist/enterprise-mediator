using System;
using System.Net;

namespace EnterpriseMediator.Core.SharedKernel.Exceptions;

/// <summary>
/// Exception thrown when a requested resource or entity cannot be found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public class NotFoundException : CustomException
{
    public NotFoundException(string message)
        : base(message, statusCode: HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.", statusCode: HttpStatusCode.NotFound)
    {
    }
}