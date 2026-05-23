using System.Net;

namespace EnterpriseMediator.Core.SharedKernel.Exceptions;

/// <summary>
/// Exception thrown when a domain business rule is violated.
/// Maps to HTTP 422 Unprocessable Entity.
/// </summary>
public class BusinessRuleException : CustomException
{
    /// <summary>
    /// The name of the business rule that was violated.
    /// </summary>
    public string? RuleName { get; }

    public BusinessRuleException(string message)
        : base(message, statusCode: HttpStatusCode.UnprocessableEntity)
    {
    }

    public BusinessRuleException(string ruleName, string message)
        : base(message, statusCode: HttpStatusCode.UnprocessableEntity)
    {
        RuleName = ruleName;
    }

    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.UnprocessableEntity)
    {
    }
}
