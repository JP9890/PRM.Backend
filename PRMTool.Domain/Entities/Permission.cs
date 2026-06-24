namespace PRMTool.Domain.Entities
{
    /// <summary>
    /// Master permission code table.
    /// Codes: MANAGE_USERS | MANAGE_EMPLOYEES | MANAGE_PROJECTS | VIEW_ALL_ALLOCATIONS |
    ///        ALLOCATE_RESOURCE | VIEW_TEAM_TIMESHEETS | SUBMIT_TIMESHEET |
    ///        VIEW_OWN_DATA | SYSTEM_CONFIG | AI_ASSISTANT
    /// </summary>
    public class Permission
    {
        public int Id { get; private set; }

        /// <summary>Unique permission code string, e.g. "MANAGE_USERS".</summary>
        public string PermissionCode { get; private set; } = string.Empty;

        public string Description { get; private set; } = string.Empty;

        protected Permission() { }

        public Permission(string permissionCode, string description)
        {
            PermissionCode = permissionCode;
            Description = description;
        }
    }
}
