namespace PRMTool.Domain.Entities
{
    /// <summary>Master project statuses: PLANNED | ACTIVE | ON_HOLD | COMPLETED</summary>
    public class ProjectStatus
    {
        public int Id { get; private set; }
        public string StatusCode { get; private set; } = string.Empty;
        public string Label { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }

        protected ProjectStatus() { }

        public ProjectStatus(string statusCode, string label, int sortOrder)
        {
            StatusCode = statusCode;
            Label = label;
            SortOrder = sortOrder;
        }
    }
}
