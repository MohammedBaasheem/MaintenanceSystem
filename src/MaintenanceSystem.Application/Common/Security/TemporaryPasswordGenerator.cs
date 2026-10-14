using System.Security.Cryptography;

namespace MaintenanceSystem.Application.Common.Security;

public static class TemporaryPasswordGenerator
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";

    public static string Generate(int length = 12)
    {
        if (length < 8)
            length = 8;

        var bytes = RandomNumberGenerator.GetBytes(length);
        var result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = Chars[bytes[i] % Chars.Length];
        }

        return new string(result);
    }
}

