namespace EnterpriseMediator.ProjectManagement.Application.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    public string DefaultConnection { get; set; } = string.Empty;
}
