using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;

namespace EnterpriseMediator.Core.SharedKernel.Exceptions;

/// <summary>
/// Exception thrown when a command or query fails business validation rules.
/// Maps to HTTP 400 Bad Request.
/// </summary>
public class ValidationException : CustomException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("One or more validation failures have occurred.", statusCode: HttpStatusCode.BadRequest)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public ValidationException(string message)
        : base(message, statusCode: HttpStatusCode.BadRequest)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.BadRequest)
    {
        Errors = new Dictionary<string, string[]>();
    }
}