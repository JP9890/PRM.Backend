using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PRMTool.Application.DTOs;
using PRMTool.Application.Helpers;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using Serilog;

namespace PRMTool.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly Serilog.ILogger _logger;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, Serilog.ILogger logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger.ForContext<AuthService>();
        }

        public async Task<AuthResponseDto?> AuthenticateAsync(string username, string password)
        {
            _logger.Information("Authentication attempt for user: {Username}", username);
            var user = await _userRepository.GetByUsernameAsync(username);

            if (!IsUserValidAndActive(user))
            {
                _logger.Warning("Authentication failed for {Username}. User not found or inactive.", username);
                return null;
            }

            if (!VerifyPassword(password, user.PasswordHash, username))
            {
                _logger.Warning("Authentication failed for {Username}. Invalid password.", username);
                return null;
            }

            _logger.Information("User {Username} successfully authenticated.", username);
            return CreateAuthResponse(user);
        }

        public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            _logger.Information("Password change attempt for user: {Username}", username);
            var user = await _userRepository.GetByUsernameAsync(username);

            if (!IsUserValidAndActive(user))
            {
                _logger.Warning("Password change failed. User {Username} not found or inactive.", username);
                return (false, "User not found or inactive.");
            }

            if (!IsNewPasswordValid(newPassword, username, out var pwdError))
            {
                return (false, pwdError);
            }

            if (!IsOldPasswordValidIfNeeded(user, oldPassword, username))
            {
                return (false, "Invalid current password.");
            }

            await UpdateUserPasswordAsync(user, newPassword);
            _logger.Information("User {Username} successfully changed their password.", username);
            return (true, string.Empty);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = CreateUserClaims(user);
            var key = GetJwtSecurityKey();
            var tokenDescriptor = CreateTokenDescriptor(claims, key);
            
            return WriteToken(tokenDescriptor);
        }

        private bool IsUserValidAndActive(User? user)
        {
            return user != null && user.IsActive;
        }

        private bool VerifyPassword(string password, string hash, string username)
        {
            try
            {
                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(hash))
                {
                    return BCrypt.Net.BCrypt.Verify(password, hash);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error verifying password for user {Username}. The hash in the database might be malformed.", username);
                return false;
            }
        }

        private AuthResponseDto CreateAuthResponse(User user)
        {
            var token = GenerateJwtToken(user);
            return new AuthResponseDto
            {
                Token = token,
                Id = user.Id,
                Username = user.Username,
                Roles = GetUserRoles(user),
                RequiresPasswordChange = user.RequiresPasswordChange
            };
        }

        private List<string> GetUserRoles(User user)
        {
            var roles = new List<string>();
            if (user.Role != null) 
            {
                roles.Add(user.Role.Name);
            }
            return roles;
        }

        private bool IsNewPasswordValid(string newPassword, string username, out string error)
        {
            if (!PasswordValidator.IsValid(newPassword, out error))
            {
                _logger.Warning("Password change failed for {Username}. {Error}", username, error);
                return false;
            }
            return true;
        }

        private bool IsOldPasswordValidIfNeeded(User user, string oldPassword, string username)
        {
            if (!VerifyPassword(oldPassword, user.PasswordHash, username))
            {
                _logger.Warning("Password change failed for {Username}. Invalid old password.", username);
                return false;
            }

            return true;
        }

        private async Task UpdateUserPasswordAsync(User user, string newPassword)
        {
            user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(newPassword));
            await _userRepository.UpdateAsync(user);
        }

        private List<Claim> CreateUserClaims(User user)
        {
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

            return claims;
        }

        private byte[] GetJwtSecurityKey()
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");
            return Encoding.ASCII.GetBytes(jwtKey);
        }

        private SecurityTokenDescriptor CreateTokenDescriptor(List<Claim> claims, byte[] key)
        {
            return new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
        }

        private double GetJwtExpirationMinutes()
        {
            return double.TryParse(_configuration["Jwt:DurationInMinutes"], out var minutes) ? minutes : 60;
        }

        private string WriteToken(SecurityTokenDescriptor tokenDescriptor)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
