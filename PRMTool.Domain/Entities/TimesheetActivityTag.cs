namespace PRMTool.Domain.Entities
{
    /// <summary>
    /// Junction table linking a TimesheetEntry to ActivityTagCatalogue.
    /// Supports both catalogue tags (activity_tag_id) and free-text custom tags.
    /// </summary>
    public class TimesheetActivityTag
    {
        public int Id { get; private set; }

        public int TimesheetEntryId { get; private set; }
        public TimesheetEntry? TimesheetEntry { get; private set; }

        /// <summary>NULL when custom_tag is used (activity type = Other).</summary>
        public int? ActivityTagId { get; private set; }
        public ActivityTagCatalogue? ActivityTag { get; private set; }

        /// <summary>Populated only when the employee selects "Other" activity type.</summary>
        public string? CustomTag { get; private set; }

        protected TimesheetActivityTag() { }

        public TimesheetActivityTag(int timesheetEntryId, int? activityTagId, string? customTag = null)
        {
            TimesheetEntryId = timesheetEntryId;
            ActivityTagId = activityTagId;
            CustomTag = customTag;
        }
    }
}
