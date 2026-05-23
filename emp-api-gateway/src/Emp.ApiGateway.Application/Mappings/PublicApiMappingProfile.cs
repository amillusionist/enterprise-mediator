using AutoMapper;
using Emp.ApiGateway.Application.Features.Projects.Queries;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;

namespace Emp.ApiGateway.Application.Mappings
{
    /// <summary>
    /// Defines mapping configurations between Internal Microservice DTOs and Public API ViewModels.
    /// This ensures decoupling between internal data structures and external contracts.
    /// </summary>
    public class PublicApiMappingProfile : Profile
    {
        public PublicApiMappingProfile()
        {
            // Project Mappings: InternalProjectDto → PublicProjectDto
            CreateMap<InternalProjectDto, PublicProjectDto>();

            // Financial Mappings: FinancialSummaryResponse → PublicFinancialSummaryDto
            CreateMap<FinancialSummaryResponse, PublicFinancialSummaryDto>()
                .ForMember(dest => dest.TotalSpent, opt => opt.MapFrom(src => src.TotalPaid))
                .ForMember(dest => dest.FinancialHealthStatus, opt => opt.MapFrom(src =>
                    src.HasOverdueInvoices ? "AtRisk" : "Healthy"));
        }
    }
}
