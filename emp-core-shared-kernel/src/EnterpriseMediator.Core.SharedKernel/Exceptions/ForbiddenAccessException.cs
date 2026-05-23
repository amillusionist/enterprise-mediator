using System.Net;

namespace EnterpriseMediator.Core.SharedKernel.Exceptions;

/// <summary>
/// Exception thrown when an authenticated user attempts an operation they are not authorized for.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public class ForbiddenAccessException : CustomException
{
    public ForbiddenAccessException()
        : base("Access is denied.", statusCode: HttpStatusCode.Forbidden)
    {
    }

    public ForbiddenAccessException(string message)
        : base(message, statusCode: HttpStatusCode.Forbidden)
    {
    }
}
