namespace PRMTool.Domain.Entities
{
    /// <summary>Master timesheet statuses: SUBMITTED | MISSED</summary>
    public class TimesheetStatus
    {
        public int Id { get; private set; }
        public string StatusCode { get; private set; } = string.Empty;
        public string Label { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }

        protected TimesheetStatus() { }

        public TimesheetStatus(string statusCode, string label, int sortOrder)
        {
            StatusCode = statusCode;
            Label = label;
            SortOrder = sortOrder;
        }
    }
}
