using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    }
}
