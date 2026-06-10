using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> CreateUserAsync(CreateUserDto dto);
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<bool> DeactivateUserAsync(int id, string currentUsername);
        Task<bool> ReactivateUserAsync(int id);
        Task<bool> ResetPasswordAsync(int id, ResetPasswordDto dto);
    }
}
