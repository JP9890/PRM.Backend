using System;
using System.Collections.Generic;
using PRMTool.Domain.Enums;

namespace PRMTool.Domain.Entities
{
    public class Employee
    {
        public int Id { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string Department { get; private set; } = string.Empty;
        public EmployeeStatus Status { get; private set; } = EmployeeStatus.BENCH;
        public bool IsActive { get; private set; } = true;
        public int? UserId { get; private set; }
        public User? User { get; private set; }
        public int? ManagerId { get; private set; }
        public User? Manager { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public ICollection<EmployeeSkill> Skills { get; private set; } = new List<EmployeeSkill>();
        public ICollection<Allocation> Allocations { get; private set; } = new List<Allocation>();

        protected Employee() { }

        public Employee(string fullName, string department, int? userId = null)
        {
            FullName = fullName;
            Department = department;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string fullName, string department)
        {
            FullName = fullName;
            Department = department;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignManager(int? managerId)
        {
            ManagerId = managerId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStatus(EmployeeStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reactivate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
