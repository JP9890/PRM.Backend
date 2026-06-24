namespace PRMTool.Domain.Entities
{
    /// <summary>Junction table linking Roles to Permissions (RBAC).</summary>
    public class RolePermission
    {
        public int Id { get; private set; }

        public int RoleId { get; private set; }
        public Role? Role { get; private set; }

        public int PermissionId { get; private set; }
        public Permission? Permission { get; private set; }

        protected RolePermission() { }

        public RolePermission(int roleId, int permissionId)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }
    }
}
