using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    public class Role
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;

        public ICollection<User> Users { get; private set; } = new List<User>();
        public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

        protected Role() { }

        public Role(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }
    }
}
