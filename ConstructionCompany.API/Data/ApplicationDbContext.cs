using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ConstructionCompany.API.Models;

namespace ConstructionCompany.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Application> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Настройка Projects
            builder.Entity<Project>(entity =>
            {
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).IsRequired().HasMaxLength(500);
                entity.Property(p => p.ImageUrl).IsRequired().HasMaxLength(200);
                entity.Property(p => p.PlanUrl).HasMaxLength(200);
                entity.Property(p => p.Specifications).IsRequired().HasMaxLength(200);
            });

            // Настройка Applications
            builder.Entity<Application>(entity =>
            {
                entity.Property(a => a.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending");

                entity.Property(a => a.ClientComments)
                    .HasMaxLength(500);

                entity.Property(a => a.ContractorComments)
                    .HasMaxLength(500);

                // Связь с User (Client)
                entity.HasOne(a => a.Client)
                    .WithMany()
                    .HasForeignKey(a => a.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Связь с Project
                entity.HasOne(a => a.Project)
                    .WithMany()
                    .HasForeignKey(a => a.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Индексы для производительности
                entity.HasIndex(a => a.ClientId);
                entity.HasIndex(a => a.ProjectId);
                entity.HasIndex(a => a.Status);
            });
        }
    }
}