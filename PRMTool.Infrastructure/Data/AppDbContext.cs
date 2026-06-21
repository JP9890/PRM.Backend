using System;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;

namespace PRMTool.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ResourceProfile> ResourceProfiles { get; set; } = null!;

        public DbSet<SkillCategory> SkillCategories { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<ProficiencyLevel> ProficiencyLevels { get; set; } = null!;
        public DbSet<ResourceSkill> ResourceSkills { get; set; } = null!;

        public DbSet<ProjectStatus> ProjectStatuses { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<MilestoneStatus> MilestoneStatuses { get; set; } = null!;
        public DbSet<Milestone> Milestones { get; set; } = null!;

        public DbSet<AllocationStatus> AllocationStatuses { get; set; } = null!;
        public DbSet<Allocation> Allocations { get; set; } = null!;

        public DbSet<ResourceUtilisation> ResourceUtilisations { get; set; } = null!;
        public DbSet<ProjectHealth> ProjectHealths { get; set; } = null!;

        public DbSet<TimesheetStatus> TimesheetStatuses { get; set; } = null!;
        public DbSet<Timesheet> Timesheets { get; set; } = null!;
        public DbSet<TimesheetEntry> TimesheetEntries { get; set; } = null!;
        public DbSet<ActivityTagCatalogue> ActivityTagCatalogues { get; set; } = null!;
        public DbSet<TimesheetActivityTag> TimesheetActivityTags { get; set; } = null!;

        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

        public DbSet<TimesheetReminderLog> TimesheetReminderLogs { get; set; } = null!;
        public DbSet<UserPermissionBlock> UserPermissionBlocks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureRbac(modelBuilder);
            ConfigureUsers(modelBuilder);
            ConfigureResourceProfile(modelBuilder);
            ConfigureSkills(modelBuilder);
            ConfigureProjects(modelBuilder);
            ConfigureAllocations(modelBuilder);
            ConfigureSchedulerSnapshots(modelBuilder);
            ConfigureTimesheets(modelBuilder);
            ConfigureSystemSettings(modelBuilder);
            ConfigureTimesheetRemindersAndPermissionBlocks(modelBuilder);

            SeedLookupData(modelBuilder);
            SeedPermissionsAndRolePermissions(modelBuilder);
            SeedAdminUser(modelBuilder);
            SeedSystemSettings(modelBuilder);
        }

        private static void ConfigureRbac(ModelBuilder m)
        {
            m.Entity<Role>().HasIndex(r => r.Name).IsUnique();

            m.Entity<Permission>().HasIndex(p => p.PermissionCode).IsUnique();

            m.Entity<RolePermission>()
                .HasOne(rp => rp.Role).WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<RolePermission>()
                .HasOne(rp => rp.Permission).WithMany()
                .HasForeignKey(rp => rp.PermissionId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
        }

        private static void ConfigureUsers(ModelBuilder m)
        {
            m.Entity<User>()
                .HasOne(u => u.Role).WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<User>().HasIndex(u => u.Username).IsUnique();
            m.Entity<User>().HasIndex(u => u.Email).IsUnique();

            m.Entity<User>().Ignore(u => u.RequiresPasswordChange);
        }

        private static void ConfigureResourceProfile(ModelBuilder m)
        {
            m.Entity<ResourceProfile>()
                .HasOne(rp => rp.User).WithOne(u => u.ResourceProfile)
                .HasForeignKey<ResourceProfile>(rp => rp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            m.Entity<ResourceProfile>()
                .HasOne(rp => rp.Manager).WithMany()
                .HasForeignKey(rp => rp.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            m.Entity<ResourceProfile>().HasIndex(rp => rp.UserId).IsUnique();
        }

        private static void ConfigureSkills(ModelBuilder m)
        {
            m.Entity<SkillCategory>().HasIndex(sc => sc.CategoryCode).IsUnique();

            m.Entity<Skill>()
                .HasOne(s => s.SkillCategory).WithMany(sc => sc.Skills)
                .HasForeignKey(s => s.SkillCategoryId).OnDelete(DeleteBehavior.Restrict);
            m.Entity<Skill>().HasIndex(s => s.Name).IsUnique();

            m.Entity<ProficiencyLevel>().HasIndex(pl => pl.LevelCode).IsUnique();

            m.Entity<ResourceSkill>()
                .HasOne(rs => rs.ResourceProfile).WithMany(rp => rp.Skills)
                .HasForeignKey(rs => rs.ResourceProfileId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<ResourceSkill>()
                .HasOne(rs => rs.Skill).WithMany()
                .HasForeignKey(rs => rs.SkillId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<ResourceSkill>()
                .HasOne(rs => rs.ProficiencyLevel).WithMany()
                .HasForeignKey(rs => rs.ProficiencyLevelId).OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureProjects(ModelBuilder m)
        {
            m.Entity<ProjectStatus>().HasIndex(ps => ps.StatusCode).IsUnique();

            m.Entity<Project>()
                .HasOne(p => p.Status).WithMany()
                .HasForeignKey(p => p.ProjectStatusId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<Project>()
                .HasOne(p => p.Manager).WithMany()
                .HasForeignKey(p => p.ManagerUserId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<MilestoneStatus>().HasIndex(ms => ms.StatusCode).IsUnique();

            m.Entity<Milestone>()
                .HasOne(ms => ms.Project).WithMany(p => p.Milestones)
                .HasForeignKey(ms => ms.ProjectId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<Milestone>()
                .HasOne(ms => ms.Status).WithMany()
                .HasForeignKey(ms => ms.MilestoneStatusId).OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureAllocations(ModelBuilder m)
        {
            m.Entity<AllocationStatus>().HasIndex(a => a.StatusCode).IsUnique();

            m.Entity<Allocation>()
                .HasOne(a => a.Resource).WithMany(rp => rp.Allocations)
                .HasForeignKey(a => a.ResourceId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<Allocation>()
                .HasOne(a => a.Project).WithMany(p => p.Allocations)
                .HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureSchedulerSnapshots(ModelBuilder m)
        {
            m.Entity<ResourceUtilisation>()
                .HasOne(ru => ru.Resource).WithMany()
                .HasForeignKey(ru => ru.ResourceId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<ResourceUtilisation>()
                .HasOne(ru => ru.AllocationStatus).WithMany()
                .HasForeignKey(ru => ru.AllocationStatusId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<ResourceUtilisation>().HasIndex(ru => ru.ResourceId).IsUnique();

            m.Entity<ProjectHealth>()
                .HasOne(ph => ph.Project).WithOne(p => p.Health)
                .HasForeignKey<ProjectHealth>(ph => ph.ProjectId).OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureTimesheets(ModelBuilder m)
        {
            m.Entity<TimesheetStatus>().HasIndex(ts => ts.StatusCode).IsUnique();

            m.Entity<Timesheet>()
                .HasOne(t => t.Resource).WithMany(rp => rp.Timesheets)
                .HasForeignKey(t => t.ResourceId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<Timesheet>()
                .HasOne(t => t.TimesheetStatus).WithMany()
                .HasForeignKey(t => t.TimesheetStatusId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<Timesheet>()
                .HasIndex(t => new { t.ResourceId, t.WeekStartDate }).IsUnique();

            m.Entity<TimesheetEntry>()
                .HasOne(te => te.Timesheet).WithMany(t => t.Entries)
                .HasForeignKey(te => te.TimesheetId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<TimesheetEntry>()
                .HasOne(te => te.Project).WithMany()
                .HasForeignKey(te => te.ProjectId).OnDelete(DeleteBehavior.Restrict);

            m.Entity<ActivityTagCatalogue>().HasIndex(a => a.TagName).IsUnique();

            m.Entity<TimesheetActivityTag>()
                .HasOne(tat => tat.TimesheetEntry).WithMany(te => te.ActivityTags)
                .HasForeignKey(tat => tat.TimesheetEntryId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<TimesheetActivityTag>()
                .HasOne(tat => tat.ActivityTag).WithMany()
                .HasForeignKey(tat => tat.ActivityTagId).OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }

        private static void ConfigureSystemSettings(ModelBuilder m)
        {
            m.Entity<SystemSetting>().HasIndex(s => s.Key).IsUnique();
        }

        private static void ConfigureTimesheetRemindersAndPermissionBlocks(ModelBuilder m)
        {
            m.Entity<TimesheetReminderLog>()
                .HasOne(r => r.Resource).WithMany()
                .HasForeignKey(r => r.ResourceId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<TimesheetReminderLog>()
                .HasIndex(r => new { r.ResourceId, r.WeekStartDate }).IsUnique();

            m.Entity<UserPermissionBlock>()
                .HasOne(b => b.User).WithMany()
                .HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<UserPermissionBlock>()
                .HasOne(b => b.Permission).WithMany()
                .HasForeignKey(b => b.PermissionId).OnDelete(DeleteBehavior.Cascade);

            m.Entity<UserPermissionBlock>()
                .HasIndex(b => new { b.UserId, b.PermissionId }).IsUnique();
        }
        private static void SeedLookupData(ModelBuilder m)
        {
            m.Entity<Role>().HasData(
                new { Id = 1, Name = "Admin",    Description = "Administrator role with full permissions", IsActive = true },
                new { Id = 2, Name = "Manager",  Description = "Manager role for managing projects and resources", IsActive = true },
                new { Id = 3, Name = "Employee", Description = "Employee role for submitting timesheets", IsActive = true }
            );

            m.Entity<SkillCategory>().HasData(
                new { Id = 1, CategoryCode = "BACKEND",  Label = "Backend",  SortOrder = 1 },
                new { Id = 2, CategoryCode = "FRONTEND", Label = "Frontend", SortOrder = 2 },
                new { Id = 3, CategoryCode = "DEVOPS",   Label = "DevOps",   SortOrder = 3 },
                new { Id = 4, CategoryCode = "QA",       Label = "QA",       SortOrder = 4 },
                new { Id = 5, CategoryCode = "OTHER",    Label = "Other",    SortOrder = 5 }
            );

            m.Entity<ProficiencyLevel>().HasData(
                new { Id = 1, LevelCode = "BEGINNER",     Label = "Beginner",     SortOrder = 1 },
                new { Id = 2, LevelCode = "INTERMEDIATE", Label = "Intermediate", SortOrder = 2 },
                new { Id = 3, LevelCode = "ADVANCED",     Label = "Advanced",     SortOrder = 3 }
            );

            m.Entity<ProjectStatus>().HasData(
                new { Id = 1, StatusCode = "PLANNED",   Label = "Planned",   SortOrder = 1 },
                new { Id = 2, StatusCode = "ACTIVE",    Label = "Active",    SortOrder = 2 },
                new { Id = 3, StatusCode = "ON_HOLD",   Label = "On Hold",   SortOrder = 3 },
                new { Id = 4, StatusCode = "COMPLETED", Label = "Completed", SortOrder = 4 }
            );

            m.Entity<MilestoneStatus>().HasData(
                new { Id = 1, StatusCode = "NOT_STARTED",  Label = "Not Started",  SortOrder = 1 },
                new { Id = 2, StatusCode = "IN_PROGRESS",  Label = "In Progress",  SortOrder = 2 },
                new { Id = 3, StatusCode = "DONE",         Label = "Done",         SortOrder = 3 }
            );

            m.Entity<AllocationStatus>().HasData(
                new { Id = 1, StatusCode = "BENCH",          Label = "Bench",           SortOrder = 1 },
                new { Id = 2, StatusCode = "ALLOCATED",      Label = "Allocated",       SortOrder = 2 },
                new { Id = 3, StatusCode = "OVER_ALLOCATED", Label = "Over Allocated",  SortOrder = 3 }
            );

            m.Entity<TimesheetStatus>().HasData(
                new { Id = 1, StatusCode = "SUBMITTED", Label = "Submitted", SortOrder = 1 },
                new { Id = 2, StatusCode = "MISSED",    Label = "Missed",    SortOrder = 2 }
            );

            m.Entity<ActivityTagCatalogue>().HasData(
                new { Id = 1,  TagName = "Backend API Development",      DisplayLabel = "Backend API Development",      SortOrder = 1  },
                new { Id = 2,  TagName = "Microservices / Architecture", DisplayLabel = "Microservices / Architecture", SortOrder = 2  },
                new { Id = 3,  TagName = "Database Design & Queries",    DisplayLabel = "Database Design & Queries",    SortOrder = 3  },
                new { Id = 4,  TagName = "WebSocket / Real-time Features", DisplayLabel = "WebSocket / Real-time Features", SortOrder = 4 },
                new { Id = 5,  TagName = "Frontend Development",         DisplayLabel = "Frontend Development",         SortOrder = 5  },
                new { Id = 6,  TagName = "Code Review / Mentoring",      DisplayLabel = "Code Review / Mentoring",      SortOrder = 6  },
                new { Id = 7,  TagName = "Bug Fixing",                   DisplayLabel = "Bug Fixing",                   SortOrder = 7  },
                new { Id = 8,  TagName = "DevOps / Deployment",          DisplayLabel = "DevOps / Deployment",          SortOrder = 8  },
                new { Id = 9,  TagName = "Testing & QA",                 DisplayLabel = "Testing & QA",                 SortOrder = 9  },
                new { Id = 10, TagName = "Documentation",                DisplayLabel = "Documentation",                SortOrder = 10 }
            );
        }

        private static void SeedPermissionsAndRolePermissions(ModelBuilder m)
        {
            // Permissions
            m.Entity<Permission>().HasData(
                new { Id = 1,  PermissionCode = "MANAGE_USERS",           Description = "Create, update, deactivate, and reset passwords for users" },
                new { Id = 2,  PermissionCode = "MANAGE_EMPLOYEES",       Description = "Manage employee profiles, skills, and manager assignments" },
                new { Id = 3,  PermissionCode = "MANAGE_PROJECTS",        Description = "Create, edit, and manage projects and milestones" },
                new { Id = 4,  PermissionCode = "VIEW_ALL_ALLOCATIONS",   Description = "View company-wide allocation matrix" },
                new { Id = 5,  PermissionCode = "ALLOCATE_RESOURCE",      Description = "Assign and end resource allocations" },
                new { Id = 6,  PermissionCode = "VIEW_TEAM_TIMESHEETS",   Description = "View timesheets for all team members" },
                new { Id = 7,  PermissionCode = "SUBMIT_TIMESHEET",       Description = "Submit weekly timesheets with hours and activity tags" },
                new { Id = 8,  PermissionCode = "VIEW_OWN_DATA",          Description = "View own allocations and timesheet history" },
                new { Id = 9,  PermissionCode = "SYSTEM_CONFIG",          Description = "Configure LLM provider, API key, scheduler, and max hours" },
                new { Id = 10, PermissionCode = "AI_ASSISTANT",           Description = "Use AI skill match and project risk summary features" }
            );

            m.Entity<RolePermission>().HasData(
                new { Id = 1,  RoleId = 1, PermissionId = 1  },
                new { Id = 2,  RoleId = 1, PermissionId = 2  },
                new { Id = 3,  RoleId = 1, PermissionId = 3  },
                new { Id = 4,  RoleId = 1, PermissionId = 4  },
                new { Id = 5,  RoleId = 1, PermissionId = 9  },
                new { Id = 6,  RoleId = 1, PermissionId = 10 }
            );

            m.Entity<RolePermission>().HasData(
                new { Id = 7,  RoleId = 2, PermissionId = 5  },
                new { Id = 8,  RoleId = 2, PermissionId = 6  },
                new { Id = 9,  RoleId = 2, PermissionId = 10 }
            );

            m.Entity<RolePermission>().HasData(
                new { Id = 10, RoleId = 3, PermissionId = 7  },
                new { Id = 11, RoleId = 3, PermissionId = 8  }
            );
        }

        private static void SeedAdminUser(ModelBuilder m)
        {
            // PasswordExpirationDate = today at seed time → forces password change on first login
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            m.Entity<User>().HasData(new
            {
                Id = 1,
                FullName = "System Admin",
                Username = "admin",
                Email = "admin@prm.local",
                Department = (string?)null,
                PasswordHash = "$2a$11$n6eLlN0jG.5EUq6tmN5eG.ULhugVu4L7o8JVWy8MT6/Qe/kpPvqm6",
                RoleId = 1,
                IsActive = true,
                PasswordExpirationDate = (DateTime?)seedDate,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });
        }

        private static void SeedSystemSettings(ModelBuilder m)
        {
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            m.Entity<SystemSetting>().HasData(
                new { Id = 1, Key = "LlmProvider",          Value = "Gemini", UpdatedAt = seedDate },
                new { Id = 2, Key = "LlmApiKey",            Value = "",       UpdatedAt = seedDate },
                new { Id = 3, Key = "SchedulerIntervalHours", Value = "4",    UpdatedAt = seedDate },
                new { Id = 4, Key = "MaxWeeklyHours",       Value = "40",     UpdatedAt = seedDate }
            );
        }
    }
}
