using System.Security.Cryptography;
using System.Text;

namespace Backend.Tools;

public static class EncryptionTools
{
    public static string GetMD5HashHexString(string fullPath)
    {
        byte[] stringBytes = Encoding.UTF8.GetBytes(fullPath.ToLower());
        byte[] hash = MD5.HashData(stringBytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
