using System.Security.Cryptography;
using System.Text;

namespace Infrastructure;

public static class PasswordHasher
{
    /**
     * Hashes a password and returns a list of two strings: the salt and the hashed password.
     */
    public static List<byte[]> HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
        byte[] hashed = SHA512.HashData(salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray());
        return new List<byte[]>()
        {
            salt,
            hashed
        };
    }

    /**
     * Hashes a password using a given salt and returns the hashed password.
     */
    public static byte[] HashPassword(string password, byte[] salt)
    {
        return SHA512.HashData(salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray());
    }
}