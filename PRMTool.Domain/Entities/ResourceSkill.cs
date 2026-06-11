using System;

namespace PRMTool.Domain.Entities
{
    /// <summary>
    /// Junction linking ResourceProfile ↔ Skills with proficiency.
    /// Renamed from EmployeeSkill. Uses FK lookup tables for skill and proficiency.
    /// </summary>
    public class ResourceSkill
    {
        public int Id { get; private set; }

        public int ResourceProfileId { get; private set; }
        public ResourceProfile? ResourceProfile { get; private set; }

        public int SkillId { get; private set; }
        public Skill? Skill { get; private set; }

        public int ProficiencyLevelId { get; private set; }
        public ProficiencyLevel? ProficiencyLevel { get; private set; }

        public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        protected ResourceSkill() { }

        public ResourceSkill(int resourceProfileId, int skillId, int proficiencyLevelId)
        {
            ResourceProfileId = resourceProfileId;
            SkillId = skillId;
            ProficiencyLevelId = proficiencyLevelId;
            AssignedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProficiency(int proficiencyLevelId)
        {
            ProficiencyLevelId = proficiencyLevelId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
