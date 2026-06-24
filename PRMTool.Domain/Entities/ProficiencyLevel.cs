namespace PRMTool.Domain.Entities
{
    /// <summary>Master proficiency levels: BEGINNER | INTERMEDIATE | ADVANCED</summary>
    public class ProficiencyLevel
    {
        public int Id { get; private set; }

        /// <summary>Unique code, e.g. "BEGINNER"</summary>
        public string LevelCode { get; private set; } = string.Empty;

        public string Label { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }

        protected ProficiencyLevel() { }

        public ProficiencyLevel(string levelCode, string label, int sortOrder)
        {
            LevelCode = levelCode;
            Label = label;
            SortOrder = sortOrder;
        }
    }
}
