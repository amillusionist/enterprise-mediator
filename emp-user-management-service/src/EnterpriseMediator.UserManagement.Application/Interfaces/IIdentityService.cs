namespace EnterpriseMediator.UserManagement.Application.Interfaces;

/// <summary>
/// Abstraction for external identity provider (AWS Cognito) operations.
/// Implementation lives in Infrastructure layer.
/// </summary>
public interface IIdentityService
{
    Task<string> CreateUserAsync(string email, string password, string role, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string identityId, CancellationToken cancellationToken = default);
    Task AssignUserToGroupAsync(string identityId, string groupName, CancellationToken cancellationToken = default);
}
