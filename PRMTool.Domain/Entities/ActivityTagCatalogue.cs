namespace PRMTool.Domain.Entities
{
    /// <summary>Master activity tag catalogue used in timesheet entries.</summary>
    public class ActivityTagCatalogue
    {
        public int Id { get; private set; }

        /// <summary>Unique tag name, e.g. "Backend API Development"</summary>
        public string TagName { get; private set; } = string.Empty;

        public string DisplayLabel { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }

        protected ActivityTagCatalogue() { }

        public ActivityTagCatalogue(string tagName, string displayLabel, int sortOrder)
        {
            TagName = tagName;
            DisplayLabel = displayLabel;
            SortOrder = sortOrder;
        }
    }
}
