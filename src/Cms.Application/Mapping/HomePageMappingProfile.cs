using AutoMapper;
using Cms.Application.DTOs.HomePage;
using Cms.Domain.Entities;

namespace Cms.Application.Mapping;

public class HomePageMappingProfile : Profile
{
    public HomePageMappingProfile()
    {
        CreateMap<HomePageSection, HomePageSectionDto>()
            .ForMember(d => d.Config, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.Config = Shared.Helpers.JsonHelper.DeserializeToObject(src.JsonData);
            });

        CreateMap<UpdateHomePageSectionDto, HomePageSection>()
            .ForAllMembers(opt => opt.Condition((_, _, srcMember) => srcMember is not null));

        CreateMap<CreateHomePageSectionDto, HomePageSection>();
    }
}
