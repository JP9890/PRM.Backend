using System.Text.RegularExpressions;

namespace PRMTool.Application.Helpers
{
    public static class PasswordValidator
    {
        private const int MaximumPasswordLength = 8;
        public static bool IsValid(string password, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < MaximumPasswordLength)
            {
                errorMessage = "Password must be at least 8 characters long.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "Password must contain at least one uppercase letter.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                errorMessage = "Password must contain at least one number.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
