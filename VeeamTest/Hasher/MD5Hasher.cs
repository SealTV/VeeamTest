using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VeeamTest.Hasher
{
    public class MD5Hasher : IHasher
    {
        private readonly MD5 md5;

        public MD5Hasher()
        {
            this.md5 = new MD5CryptoServiceProvider();
        }

        public string GetHash(byte[] input, out byte[] output)
        {
            output = this.md5.ComputeHash(input);

            StringBuilder builder = new StringBuilder();
            foreach(var @byte in output)
            {
                builder.Append(@byte.ToString("x2"));
            }

            return builder.ToString();
        }

        public bool VerifyHash(byte[] input, byte[] hash)
        {
            var testHash = this.md5.ComputeHash(input);
            var result = hash.SequenceEqual(testHash);
            return result;
        }

    }
}
