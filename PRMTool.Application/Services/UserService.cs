using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PRMTool.Application.DTOs;
using PRMTool.Application.Helpers;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IEmployeeRepository employeeRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto?> CreateUserAsync(CreateUserDto dto)
        {
            _logger.LogInformation("Attempting to create user: {Username}", dto.Username);

            if (!PasswordValidator.IsValid(dto.Password, out var passwordError))
                throw new InvalidOperationException(passwordError);

            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
                throw new InvalidOperationException("Username already exists.");

            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException("Email already exists.");

            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (role == null)
                throw new InvalidOperationException("Role does not exist.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User(dto.FullName, dto.Username, dto.Email, hashedPassword, dto.RoleId);
            user.FlagForPasswordChange();

            await _userRepository.AddAsync(user);

            if (role.Name == "Employee")
            {
                var employee = new Employee(dto.FullName, "General", user.Id);
                await _employeeRepository.AddAsync(employee);
            }

            var createdUser = await _userRepository.GetByIdAsync(user.Id);
            return MapToDto(createdUser!);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null && existingEmail.Id != id)
                throw new InvalidOperationException("Email already in use by another account.");

            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (role == null)
                throw new InvalidOperationException("Role does not exist.");

            user.UpdateDetails(dto.FullName, dto.Email, dto.RoleId, dto.IsActive);
            await _userRepository.UpdateAsync(user);

            var updatedUser = await _userRepository.GetByIdAsync(user.Id);
            return MapToDto(updatedUser!);
        }

        public async Task<bool> DeactivateUserAsync(int id, string currentUsername)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            if (user.Username == currentUsername)
                throw new InvalidOperationException("You cannot deactivate your own account.");

            if (user.Role?.Name == "Admin" || user.RoleId == 1)
            {
                var allUsers = await _userRepository.GetAllAsync();
                var activeAdminsCount = allUsers.Count(u => u.IsActive && (u.Role?.Name == "Admin" || u.RoleId == 1));
                if (activeAdminsCount <= 1)
                    throw new InvalidOperationException("You cannot deactivate the last active Admin.");
            }

            user.Deactivate();
            await _userRepository.UpdateAsync(user);

            var employee = await _employeeRepository.GetByUserIdAsync(id);
            if (employee != null)
            {
                employee.Deactivate();
                await _employeeRepository.UpdateAsync(employee);
            }

            return true;
        }

        public async Task<bool> ReactivateUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            user.Activate();
            await _userRepository.UpdateAsync(user);

            var employee = await _employeeRepository.GetByUserIdAsync(id);
            if (employee != null)
            {
                employee.Reactivate();
                await _employeeRepository.UpdateAsync(employee);
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(int id, ResetPasswordDto dto)
        {
            if (!PasswordValidator.IsValid(dto.NewPassword, out var passwordError))
                throw new InvalidOperationException(passwordError);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            user.ResetPassword(BCrypt.Net.BCrypt.HashPassword(dto.NewPassword));
            await _userRepository.UpdateAsync(user);
            return true;
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name
            };
        }
    }
}
