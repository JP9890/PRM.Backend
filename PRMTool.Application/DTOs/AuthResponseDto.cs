using System.Collections.Generic;

namespace PRMTool.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public bool RequiresPasswordChange { get; set; }
    }
}
