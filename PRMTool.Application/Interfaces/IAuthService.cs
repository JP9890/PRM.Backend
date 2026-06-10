using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> AuthenticateAsync(string username, string password);
        Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword);
    }
}
