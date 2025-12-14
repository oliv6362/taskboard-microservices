namespace ProjectService.Domain.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int OwnerUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
