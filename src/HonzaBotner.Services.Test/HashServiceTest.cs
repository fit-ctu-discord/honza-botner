using HonzaBotner.Services.Contract;
using Xunit;

namespace HonzaBotner.Services.Test
{
    public class HashServiceTest
    {
        [Theory]
        [InlineData("bittnja3", "64f971f16819e3039a4fd234e674be64a26ecedf4f669a1700338ac348d76a48")]
        public void HashTest(string input, string hash)
        {
            IHashService hashService = new Sha256HashService();

            string output = hashService.Hash(input);

            Assert.Equal(hash, output);
        }
    }
}
