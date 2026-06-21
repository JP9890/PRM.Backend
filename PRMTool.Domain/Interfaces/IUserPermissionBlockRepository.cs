using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface IUserPermissionBlockRepository
    {
        /// <summary>Returns true if the user has an active block for the given permission code (e.g. "SUBMIT_TIMESHEET").</summary>
        Task<bool> IsBlockedAsync(int userId, string permissionCode);

        /// <summary>Inserts a new permission block row for the user.</summary>
        Task AddAsync(UserPermissionBlock block);

        /// <summary>Removes the permission block for the user. No-op if not found.</summary>
        Task RemoveAsync(int userId, string permissionCode);
    }
}
