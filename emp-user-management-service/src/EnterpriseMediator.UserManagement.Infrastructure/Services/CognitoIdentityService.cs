using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using EnterpriseMediator.UserManagement.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Services;

public class CognitoIdentityService : IIdentityService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly string _userPoolId;
    private readonly ILogger<CognitoIdentityService> _logger;

    public CognitoIdentityService(
        IAmazonCognitoIdentityProvider cognitoClient,
        IConfiguration configuration,
        ILogger<CognitoIdentityService> logger)
    {
        _cognitoClient = cognitoClient ?? throw new ArgumentNullException(nameof(cognitoClient));
        _userPoolId = configuration["AWS:UserPoolId"]
            ?? throw new InvalidOperationException("AWS:UserPoolId configuration is missing.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> CreateUserAsync(string email, string password, string role, CancellationToken cancellationToken = default)
    {
        var request = new AdminCreateUserRequest
        {
            UserPoolId = _userPoolId,
            Username = email,
            TemporaryPassword = password,
            UserAttributes = new List<AttributeType>
            {
                new() { Name = "email", Value = email },
                new() { Name = "email_verified", Value = "true" }
            },
            MessageAction = MessageActionType.SUPPRESS
        };

        var response = await _cognitoClient.AdminCreateUserAsync(request, cancellationToken);

        _logger.LogInformation("Created Cognito user for {Email} with sub {Sub}",
            email, response.User.Username);

        await AssignUserToGroupAsync(response.User.Username, role, cancellationToken);

        return response.User.Username;
    }

    public async Task DeleteUserAsync(string identityId, CancellationToken cancellationToken = default)
    {
        var request = new AdminDeleteUserRequest
        {
            UserPoolId = _userPoolId,
            Username = identityId
        };

        await _cognitoClient.AdminDeleteUserAsync(request, cancellationToken);

        _logger.LogInformation("Deleted Cognito user {IdentityId}", identityId);
    }

    public async Task AssignUserToGroupAsync(string identityId, string groupName, CancellationToken cancellationToken = default)
    {
        var request = new AdminAddUserToGroupRequest
        {
            UserPoolId = _userPoolId,
            Username = identityId,
            GroupName = groupName
        };

        await _cognitoClient.AdminAddUserToGroupAsync(request, cancellationToken);

        _logger.LogDebug("Assigned Cognito user {IdentityId} to group {GroupName}", identityId, groupName);
    }
}
