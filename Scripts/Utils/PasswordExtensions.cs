using System;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{
    public static class PasswordExtensions
    {
        private const string Salt = "PlayVibePasswordExtensions";
        
        public static string ToHashPassword(this string password)
        {
            using var sha256 = SHA256.Create();
            var saltedPasswordBytes = Encoding.UTF8.GetBytes(password + Salt);
            var hashedBytes = sha256.ComputeHash(saltedPasswordBytes);
            
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}