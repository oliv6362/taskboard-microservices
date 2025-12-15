using Microsoft.EntityFrameworkCore;
using ProjectService.Domain.Entities;

namespace ProjectService.Infrastructure.Data
{
    public class ProjectDbContext : DbContext
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>(e =>
            {
                e.ToTable("Project");
                e.HasKey(p => p.ProjectId);
                e.Property(p => p.ProjectId).UseIdentityColumn();
                e.HasIndex(p => p.OwnerUserId);

            });
        }
    }
}
