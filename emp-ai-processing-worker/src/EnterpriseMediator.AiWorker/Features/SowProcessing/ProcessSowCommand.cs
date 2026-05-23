using MediatR;

namespace EnterpriseMediator.AiWorker.Features.SowProcessing;

/// <summary>
/// Command to initiate the processing of an uploaded SOW document.
/// </summary>
/// <param name="SowId">The unique identifier of the SOW record in the database.</param>
/// <param name="FileKey">The object key (path) of the file in the storage system (S3).</param>
public record ProcessSowCommand(Guid SowId, string FileKey) : IRequest<ProcessSowResult>;

/// <summary>
/// Result of SOW processing, indicating success or failure with an error message.
/// </summary>
public record ProcessSowResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static ProcessSowResult Success() => new() { IsSuccess = true };
    public static ProcessSowResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}