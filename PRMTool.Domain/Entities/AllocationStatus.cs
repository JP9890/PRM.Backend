namespace PRMTool.Domain.Entities
{
    /// <summary>Master allocation statuses: BENCH | ALLOCATED | OVER_ALLOCATED</summary>
    public class AllocationStatus
    {
        public int Id { get; private set; }
        public string StatusCode { get; private set; } = string.Empty;
        public string Label { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }

        protected AllocationStatus() { }

        public AllocationStatus(string statusCode, string label, int sortOrder)
        {
            StatusCode = statusCode;
            Label = label;
            SortOrder = sortOrder;
        }
    }
}
