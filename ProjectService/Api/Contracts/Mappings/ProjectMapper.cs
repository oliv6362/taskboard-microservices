using ProjectService.Domain.Entities;   
using ProjectService.Api.Contracts.Dtos;

namespace ProjectService.Api.Contracts.Mappings
{
    public static class ProjectMapper
    {
        public static Project ToEntity (this CreateProjectRequest dto) =>
            new()
            {             
                Name = dto.Name,
                Description = dto.Description
            };
    }
}
