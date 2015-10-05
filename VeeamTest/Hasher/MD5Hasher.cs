using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VeeamTest.Hasher
{
    public class MD5Hasher : Hasher
    {
        private readonly MD5 md5;

        public MD5Hasher()
        {
            this.md5 = new MD5CryptoServiceProvider();
        }

        public override byte[] GetHash(byte[] input)
        {
            return this.md5.ComputeHash(input);
        }

        public override bool VerifyHash(byte[] originData, byte[] hash)
        {
            var testHash = this.md5.ComputeHash(originData);
            var result = hash.SequenceEqual(testHash);
            return result;
        }

    }
}
