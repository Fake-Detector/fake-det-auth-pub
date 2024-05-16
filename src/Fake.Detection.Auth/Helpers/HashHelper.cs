using System.Security.Cryptography;
using System.Text;

namespace Fake.Detection.Auth.Helpers;

public static class HashHelper
{
    public static string ComputeSha256Hash(this string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

        var builder = new StringBuilder();
        
        foreach (var item in bytes)
        {
            builder.Append(item.ToString("x2"));
        }
        
        return builder.ToString();
    }
}