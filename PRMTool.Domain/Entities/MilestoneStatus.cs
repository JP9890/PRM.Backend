namespace PRMTool.Domain.Entities
{
    /// <summary>Master milestone statuses: NOT_STARTED | IN_PROGRESS | DONE</summary>
    public class MilestoneStatus
    {
        public int Id { get; private set; }
        public string StatusCode { get; private set; } = string.Empty;
        public string Label { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }

        protected MilestoneStatus() { }

        public MilestoneStatus(string statusCode, string label, int sortOrder)
        {
            StatusCode = statusCode;
            Label = label;
            SortOrder = sortOrder;
        }
    }
}
