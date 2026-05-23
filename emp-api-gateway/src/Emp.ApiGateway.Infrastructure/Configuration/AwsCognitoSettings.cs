namespace Emp.ApiGateway.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration settings for AWS Cognito authentication.
    /// Bound to the "AWS:Cognito" section in appsettings.json.
    /// </summary>
    public class AwsCognitoSettings
    {
        /// <summary>
        /// The AWS Cognito User Pool ID.
        /// </summary>
        public string UserPoolId { get; set; } = string.Empty;

        /// <summary>
        /// The AWS Cognito App Client ID.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;
    }
}
