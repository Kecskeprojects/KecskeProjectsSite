using Backend.Database.Model;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace Backend.Tools;

public static class HashTools
{
    public static byte[] GetHashBytes(Account account, string password)
    {
        PasswordHasher<Account> hasher = new();
        string hash = hasher.HashPassword(account, password);

        return Encoding.UTF8.GetBytes(hash);
    }
    public static bool VerifyPassword(Account account, string rawPassword, byte[] hashedPassword)
    {
        PasswordHasher<Account> hasher = new();

        string hashedPasswordString = Encoding.UTF8.GetString(hashedPassword);
        PasswordVerificationResult result = hasher.VerifyHashedPassword(account, hashedPasswordString, rawPassword);

        return result != PasswordVerificationResult.Failed;
    }
}
