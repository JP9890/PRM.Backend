using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;

namespace PRMTool.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<EmployeeSkill> EmployeeSkills { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Milestone> Milestones { get; set; } = null!;
        public DbSet<Allocation> Allocations { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<Timesheet> Timesheets { get; set; } = null!;
        public DbSet<TimesheetEntry> TimesheetEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<EmployeeSkill>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.Skills)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Manager)
                .WithMany()
                .HasForeignKey(p => p.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Milestone>()
                .HasOne(m => m.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Allocation>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Allocations)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Allocation>()
                .HasOne(a => a.Project)
                .WithMany(p => p.Allocations)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SystemSetting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            modelBuilder.Entity<Timesheet>()
                .HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimesheetEntry>()
                .HasOne(e => e.Timesheet)
                .WithMany(t => t.Entries)
                .HasForeignKey(e => e.TimesheetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimesheetEntry>()
                .HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>().HasData(
                new { Id = 1, Name = "Admin", Description = "Administrator role with full permissions" },
                new { Id = 2, Name = "Manager", Description = "Manager role for managing projects and resources" },
                new { Id = 3, Name = "Employee", Description = "Employee role for submitting timesheets" }
            );

            modelBuilder.Entity<User>().HasData(
                new {
                    Id = 1,
                    FullName = "System Admin",
                    Username = "admin",
                    Email = "admin@prm.local",
                    PasswordHash = "$2a$11$n6eLlN0jG.5EUq6tmN5eG.ULhugVu4L7o8JVWy8MT6/Qe/kpPvqm6",
                    RoleId = 1,
                    IsActive = true,
                    RequiresPasswordChange = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<SystemSetting>().HasData(
                new { Id = 1, Key = "LlmProvider", Value = "Gemini", UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 2, Key = "LlmApiKey", Value = "", UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 3, Key = "SchedulerIntervalHours", Value = "4", UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 4, Key = "MaxWeeklyHours", Value = "40", UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
