using AssignmentService.Domain.Entities;
using AssignmentService.Api.Contracts.Dtos;

namespace AssignmentService.Api.Contracts.Mappings
{
    public static class AssignmentMapper
    {
        public static Assignment ToEntity (this CreateAssignmentRequest dto) =>
            new()
            {
                Title = dto.Title,
                Description = dto.Description,
                ProjectId = dto.ProjectId
            };

        public static void ApplyUpdate (this Assignment assignment, UpdateAssignmentRequest dto)
        {
            assignment.Status = dto.Status;
        }
    }
}
