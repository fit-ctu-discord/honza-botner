using System.Security.Cryptography;
using System.Text;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Services;

public class Sha256HashService : IHashService
{
    public string Hash(string input)
    {
        byte[] toBeHashed = Encoding.UTF8.GetBytes(input);
        byte[] bytes = SHA256.HashData(toBeHashed);

        StringBuilder strB = new(64);
        foreach (byte b in bytes)
        {
            strB.Append(b.ToString("x2"));
        }

        return strB.ToString();
    }
}
