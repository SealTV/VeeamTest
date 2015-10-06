using System.Linq;
using System.Security.Cryptography;

namespace VeeamTest.Hasher
{

    public class SHA256Hasher : Hasher
    {
        private readonly SHA256 sha256;

        public SHA256Hasher()
        {
            this.sha256 = new SHA256CryptoServiceProvider();
        }

        public override byte[] GetHash(byte[] input)
        {
            return this.sha256.ComputeHash(input);
        }

        public override bool VerifyHash(byte[] originData, byte[] hash)
        {
            var testHash = this.sha256.ComputeHash(originData);
            var result = hash.SequenceEqual(testHash);
            return result;
        }
    }
}
