using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PRMTool.Application.DTOs;
using PRMTool.Application.Helpers;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> AuthenticateAsync(string username, string password)
        {
            _logger.LogInformation("Authentication attempt for user: {Username}", username);

            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Authentication failed for {Username}. User not found or inactive.", username);
                return null;
            }

            bool isPasswordValid = false;
            try
            {
                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(user.PasswordHash))
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password for user {Username}. The hash in the database might be malformed.", username);
            }

            if (!isPasswordValid)
            {
                _logger.LogWarning("Authentication failed for {Username}. Invalid password.", username);
                return null;
            }

            var token = GenerateJwtToken(user);

            var roles = new List<string>();
            if (user.Role != null) roles.Add(user.Role.Name);

            _logger.LogInformation("User {Username} successfully authenticated.", username);

            return new AuthResponseDto
            {
                Token = token,
                Id = user.Id,
                Username = user.Username,
                Roles = roles,
                RequiresPasswordChange = user.RequiresPasswordChange
            };
        }

        public async Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            _logger.LogInformation("Password change attempt for user: {Username}", username);

            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Password change failed. User {Username} not found or inactive.", username);
                return false;
            }

            if (!PasswordValidator.IsValid(newPassword, out var passwordError))
            {
                _logger.LogWarning("Password change failed for {Username}. {Error}", username, passwordError);
                return false;
            }

            if (!user.RequiresPasswordChange)
            {
                bool isPasswordValid = false;
                try
                {
                    if (!string.IsNullOrEmpty(oldPassword) && !string.IsNullOrEmpty(user.PasswordHash))
                    {
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying old password for user {Username}. The hash in the database might be malformed.", username);
                }

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Password change failed for {Username}. Invalid old password.", username);
                    return false;
                }
            }

            user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(newPassword));
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User {Username} successfully changed their password.", username);
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");
            var key = Encoding.ASCII.GetBytes(jwtKey);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            }
            
            if (user.RequiresPasswordChange)
            {
                claims.Add(new Claim("RequiresPasswordChange", "true"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"] ?? "60")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
