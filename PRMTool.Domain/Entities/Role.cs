using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    public class Role
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        
        public ICollection<User> Users { get; private set; } = new List<User>();

        // Parameterless constructor for EF Core
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
