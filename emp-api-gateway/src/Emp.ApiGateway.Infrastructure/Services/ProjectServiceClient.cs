using System.Net;
using System.Net.Http.Json;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Infrastructure.Configuration;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Projects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emp.ApiGateway.Infrastructure.Services;

/// <summary>
/// HTTP Client implementation for communicating with the Project Microservice.
/// Utilizes HttpClientFactory and configured resilience policies.
/// </summary>
public class ProjectServiceClient : IProjectServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectServiceClient> _logger;

    public ProjectServiceClient(
        HttpClient httpClient,
        IOptions<ServiceUrls> serviceUrls,
        ILogger<ProjectServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var urls = serviceUrls?.Value ?? throw new ArgumentNullException(nameof(serviceUrls));

        if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(urls.ProjectService))
        {
            _httpClient.BaseAddress = new Uri(urls.ProjectService);
        }
    }

    public async Task<InternalProjectDto?> GetProjectDetailsAsync(Guid projectId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching project details for ProjectId: {ProjectId}", projectId);

        var response = await _httpClient.GetAsync($"api/v1/projects/{projectId}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<InternalProjectDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Project service returned 404 for ProjectId: {ProjectId}", projectId);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Project service call failed. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Project service call failed with status code {response.StatusCode}");
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid projectId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching project by ID: {ProjectId}", projectId);

        var response = await _httpClient.GetAsync($"api/v1/projects/{projectId}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Project not found for ProjectId: {ProjectId}", projectId);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Get project by ID failed. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get project by ID failed with status code {response.StatusCode}");
    }

    public async Task<PagedResultDto<ProjectDto>> GetProjectsAsync(int page, int pageSize, string? search, string? status, Guid? clientId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching projects list. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var queryParams = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) queryParams += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(status)) queryParams += $"&status={Uri.EscapeDataString(status)}";
        if (clientId.HasValue) queryParams += $"&clientId={clientId.Value}";

        var response = await _httpClient.GetAsync($"api/v1/projects{queryParams}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PagedResultDto<ProjectDto>>(cancellationToken: ct)
                   ?? new PagedResultDto<ProjectDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Get projects failed. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get projects failed with status code {response.StatusCode}");
    }

    public async Task<Guid> CreateProjectAsync(CreateProjectDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Creating project with name: {ProjectName}", dto.Name);

        var response = await _httpClient.PostAsJsonAsync("api/v1/projects", dto, ct);

        if (response.IsSuccessStatusCode)
        {
            var projectId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: ct);
            _logger.LogInformation("Project created successfully with ID: {ProjectId}", projectId);
            return projectId;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to create project. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Create project failed with status code {response.StatusCode}: {content}");
    }

    public async Task UpdateProjectStatusAsync(Guid projectId, string status, CancellationToken ct)
    {
        _logger.LogInformation("Updating project status. ProjectId: {ProjectId}, Status: {Status}", projectId, status);

        var response = await _httpClient.PatchAsJsonAsync($"api/v1/projects/{projectId}/status", new { Status = status }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to update project status. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Update project status failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task UploadSowAsync(Guid projectId, Stream fileStream, string fileName, string contentType, CancellationToken ct)
    {
        _logger.LogInformation("Uploading SOW for ProjectId: {ProjectId}, FileName: {FileName}", projectId, fileName);

        using var formContent = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        formContent.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync($"api/v1/projects/{projectId}/sow", formContent, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to upload SOW. StatusCode: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            throw new HttpRequestException($"SOW upload failed with status code {response.StatusCode}: {errorContent}");
        }

        _logger.LogInformation("Successfully uploaded SOW for ProjectId: {ProjectId}", projectId);
    }

    public async Task<SowDocumentDto?> GetSowStatusAsync(Guid projectId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching SOW status for ProjectId: {ProjectId}", projectId);

        var response = await _httpClient.GetAsync($"api/v1/projects/{projectId}/sow/status", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SowDocumentDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("SOW not found for ProjectId: {ProjectId}", projectId);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch SOW status. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get SOW status failed with status code {response.StatusCode}");
    }

    public async Task<ProjectBriefDto?> GetProjectBriefAsync(Guid projectId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching project brief for ProjectId: {ProjectId}", projectId);

        var response = await _httpClient.GetAsync($"api/v1/projects/{projectId}/brief", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ProjectBriefDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Project brief not found for ProjectId: {ProjectId}", projectId);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch project brief. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get project brief failed with status code {response.StatusCode}");
    }

    public async Task<ProjectBriefDto> ApproveProjectBriefAsync(Guid projectId, UpdateProjectBriefRequest? edits, CancellationToken ct)
    {
        _logger.LogInformation("Approving project brief for ProjectId: {ProjectId}", projectId);

        var response = await _httpClient.PutAsJsonAsync($"api/v1/projects/{projectId}/brief/approve", edits, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ProjectBriefDto>(cancellationToken: ct);
            _logger.LogInformation("Project brief approved for ProjectId: {ProjectId}", projectId);
            return result!;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to approve project brief. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Approve project brief failed with status code {response.StatusCode}: {content}");
    }

    public async Task UpdateBriefAsync(Guid projectId, UpdateProjectBriefRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Updating brief for ProjectId: {ProjectId}", projectId);

        var response = await _httpClient.PutAsJsonAsync($"api/v1/projects/{projectId}/brief", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to update brief. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Update brief failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task DistributeBriefAsync(Guid projectId, Guid[] vendorIds, CancellationToken ct)
    {
        _logger.LogInformation("Distributing brief for ProjectId: {ProjectId} to {VendorCount} vendors", projectId, vendorIds.Length);

        var response = await _httpClient.PostAsJsonAsync($"api/v1/projects/{projectId}/distribute", new { VendorIds = vendorIds }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to distribute brief. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Distribute brief failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task AwardProjectAsync(Guid projectId, Guid vendorId, CancellationToken ct)
    {
        _logger.LogInformation("Awarding project {ProjectId} to vendor {VendorId}", projectId, vendorId);

        var response = await _httpClient.PostAsJsonAsync($"api/v1/projects/{projectId}/award", new { VendorId = vendorId }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to award project. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Award project failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task<IReadOnlyList<VendorMatchResultDto>> GetMatchingVendorsAsync(Guid projectId, int maxResults, double minScore, CancellationToken ct)
    {
        _logger.LogInformation("Fetching matching vendors for ProjectId: {ProjectId}, MaxResults: {MaxResults}, MinScore: {MinScore}",
            projectId, maxResults, minScore);

        var response = await _httpClient.GetAsync(
            $"api/v1/projects/{projectId}/matching-vendors?maxResults={maxResults}&minScore={minScore}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<VendorMatchResultDto>>(cancellationToken: ct) ?? [];
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No matching vendors found for ProjectId: {ProjectId}", projectId);
            return [];
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch matching vendors. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get matching vendors failed with status code {response.StatusCode}");
    }

    public async Task<IReadOnlyList<MilestoneDto>> GetProjectMilestonesAsync(Guid projectId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching milestones for ProjectId: {ProjectId}", projectId);

        var response = await _httpClient.GetAsync($"api/v1/projects/{projectId}/milestones", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<MilestoneDto>>(cancellationToken: ct) ?? [];
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch milestones. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get milestones failed with status code {response.StatusCode}");
    }

    public async Task<MilestoneDto?> GetMilestoneAsync(Guid milestoneId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching milestone: {MilestoneId}", milestoneId);

        var response = await _httpClient.GetAsync($"api/v1/milestones/{milestoneId}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<MilestoneDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch milestone. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get milestone failed with status code {response.StatusCode}");
    }

    public async Task<MilestoneDto> ApproveMilestoneAsync(Guid milestoneId, CancellationToken ct)
    {
        _logger.LogInformation("Approving milestone: {MilestoneId}", milestoneId);

        var response = await _httpClient.PutAsync($"api/v1/milestones/{milestoneId}/approve", null, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<MilestoneDto>(cancellationToken: ct);
            _logger.LogInformation("Milestone approved: {MilestoneId}", milestoneId);
            return result!;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to approve milestone. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Approve milestone failed with status code {response.StatusCode}: {content}");
    }

    public async Task<MilestoneDto> RejectMilestoneAsync(Guid milestoneId, string reason, CancellationToken ct)
    {
        _logger.LogInformation("Rejecting milestone: {MilestoneId}, Reason: {Reason}", milestoneId, reason);

        var response = await _httpClient.PutAsJsonAsync($"api/v1/milestones/{milestoneId}/reject", new { Reason = reason }, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<MilestoneDto>(cancellationToken: ct);
            _logger.LogInformation("Milestone rejected: {MilestoneId}", milestoneId);
            return result!;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to reject milestone. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Reject milestone failed with status code {response.StatusCode}: {content}");
    }

    public async Task<IReadOnlyList<ProposalDto>> GetProjectProposalsAsync(Guid projectId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching proposals for ProjectId: {ProjectId}", projectId);

        var response = await _httpClient.GetAsync($"api/v1/projects/{projectId}/proposals", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<ProposalDto>>(cancellationToken: ct) ?? [];
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch proposals. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get proposals failed with status code {response.StatusCode}");
    }

    public async Task UpdateProposalStatusAsync(Guid proposalId, string status, string? notes, CancellationToken ct)
    {
        _logger.LogInformation("Updating proposal status. ProposalId: {ProposalId}, Status: {Status}", proposalId, status);

        var response = await _httpClient.PutAsJsonAsync($"api/v1/proposals/{proposalId}/status", new { Status = status, Notes = notes }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to update proposal status. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Update proposal status failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task AwardProposalAsync(Guid proposalId, CancellationToken ct)
    {
        _logger.LogInformation("Awarding proposal: {ProposalId}", proposalId);

        var response = await _httpClient.PostAsync($"api/v1/proposals/{proposalId}/award", null, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to award proposal. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Award proposal failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task SubmitProposalAsync(string token, Stream? fileStream, string? fileName, decimal cost, string timeline, string keyPersonnel, CancellationToken ct)
    {
        _logger.LogInformation("Submitting proposal via portal token");

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(cost.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Cost");
        formContent.Add(new StringContent(timeline), "Timeline");
        formContent.Add(new StringContent(keyPersonnel), "KeyPersonnel");

        if (fileStream != null && !string.IsNullOrEmpty(fileName))
        {
            var streamContent = new StreamContent(fileStream);
            formContent.Add(streamContent, "File", fileName);
        }

        var response = await _httpClient.PostAsync($"api/v1/public/proposals/{Uri.EscapeDataString(token)}/submit", formContent, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to submit proposal. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Submit proposal failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task<ProjectBriefDto?> GetPortalBriefAsync(string token, CancellationToken ct)
    {
        _logger.LogInformation("Fetching portal brief via token");

        var response = await _httpClient.GetAsync($"api/v1/public/proposals/portal/{Uri.EscapeDataString(token)}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ProjectBriefDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch portal brief. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get portal brief failed with status code {response.StatusCode}");
    }
}
