using System.Security.Cryptography;
using System.Text;
using HonzaBotner.Services.Contract;

namespace HonzaBotner.Services
{
    public class Sha256HashService : IHashService
    {
        public string Hash(string input)
        {
            byte[] toBeHashed = Encoding.UTF8.GetBytes(input);
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(toBeHashed);

            StringBuilder strB = new();
            foreach (byte b in bytes)
            {
                strB.Append(b.ToString("x2"));
            }

            return strB.ToString();
        }
    }
}
