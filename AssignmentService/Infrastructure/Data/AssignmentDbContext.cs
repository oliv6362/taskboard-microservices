using Microsoft.EntityFrameworkCore;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.Data
{
    public class AssignmentDbContext : DbContext
    {
        public AssignmentDbContext(DbContextOptions<AssignmentDbContext> options) : base(options) { }
        public DbSet<Assignment> Assignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Assignment>(e =>
            {
                e.ToTable("Assignment");
                e.HasKey(a => a.AssignmentId);
                e.Property(a => a.AssignmentId).UseIdentityColumn();
                e.HasIndex(a => a.ProjectId);
            });
        }
    }   
}
