namespace EnterpriseMediator.ProjectManagement.Application.Configuration;

public sealed class AwsOptions
{
    public const string SectionName = "Aws";
    public string Region { get; set; } = "us-east-1";
    public string SowBucketName { get; set; } = string.Empty;
}
