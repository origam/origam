using System.Security.Cryptography;
using System.Text;
using Origam.Composer.Interfaces.Services;

namespace Origam.Composer.Services;

public class PasswordGeneratorService : IPasswordGeneratorService
{
    public string Generate(int length)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-";
        var result = new StringBuilder(length);
        using (var rng = RandomNumberGenerator.Create())
        {
            var uintBuffer = new byte[sizeof(uint)];

            while (result.Length < length)
            {
                rng.GetBytes(uintBuffer);
                var num = BitConverter.ToUInt32(uintBuffer, 0);
                result.Append(validChars[(int)(num % (uint)validChars.Length)]);
            }
        }

        return result.ToString();
    }
}
