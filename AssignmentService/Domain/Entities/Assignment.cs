namespace AssignmentService.Domain.Entities
{
    public enum AssignmentStatus
    {
        ToDo,
        InProgress,
        Done
    }

    public class Assignment
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.ToDo;
        public int ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
