namespace PRMTool.Domain.Entities
{
    /// <summary>Master skill list. Each skill belongs to a SkillCategory.</summary>
    public class Skill
    {
        public int Id { get; private set; }
        public int SkillCategoryId { get; private set; }
        public SkillCategory? SkillCategory { get; private set; }

        /// <summary>Unique skill name, e.g. "Java", "React", "Docker"</summary>
        public string Name { get; private set; } = string.Empty;

        protected Skill() { }

        public Skill(int skillCategoryId, string name)
        {
            SkillCategoryId = skillCategoryId;
            Name = name;
        }
    }
}
