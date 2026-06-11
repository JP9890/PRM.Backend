using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    /// <summary>Master list of skill categories: BACKEND | FRONTEND | DEVOPS | QA | OTHER</summary>
    public class SkillCategory
    {
        public int Id { get; private set; }

        /// <summary>Unique code, e.g. "BACKEND"</summary>
        public string CategoryCode { get; private set; } = string.Empty;

        public string Label { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }

        public ICollection<Skill> Skills { get; private set; } = new List<Skill>();

        protected SkillCategory() { }

        public SkillCategory(string categoryCode, string label, int sortOrder)
        {
            CategoryCode = categoryCode;
            Label = label;
            SortOrder = sortOrder;
        }
    }
}
