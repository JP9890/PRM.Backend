using System;
using PRMTool.Domain.Enums;

namespace PRMTool.Domain.Entities
{
    public class EmployeeSkill
    {
        public int Id { get; private set; }
        public int EmployeeId { get; private set; }
        public Employee? Employee { get; private set; }
        public string SkillName { get; private set; } = string.Empty;
        public SkillCategory Category { get; private set; }
        public ProficiencyLevel Proficiency { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        protected EmployeeSkill() { }

        public EmployeeSkill(int employeeId, string skillName, SkillCategory category, ProficiencyLevel proficiency)
        {
            EmployeeId = employeeId;
            SkillName = skillName;
            Category = category;
            Proficiency = proficiency;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateProficiency(ProficiencyLevel proficiency)
        {
            Proficiency = proficiency;
        }
    }
}
