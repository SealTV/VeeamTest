using System.Text;

namespace VeeamTest.Hasher
{
    public abstract class Hasher
    {
        public static Hasher GetHasher(HashTypes hashType)
        {
            switch(hashType)
            {
                case HashTypes.SHA256:
                    return new SHA256Hasher();
                case HashTypes.MD5:
                    return new MD5Hasher();
                default:
                    return new MD5Hasher();
            }
        }

        public static string ToString(byte[] hash)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var @byte in hash)
            {
                builder.Append(@byte.ToString("x2"));
            }

            return builder.ToString();
        }


        public static int GetHashSize(HashTypes hashType)
        {
            switch(hashType)
            {
                case HashTypes.SHA256:
                    return 32;
                case HashTypes.MD5:
                    return 16;
                default:
                    return 16;
            }
        }


        public abstract byte[] GetHash(byte[] input);
        public abstract bool VerifyHash(byte[] originData, byte[] hash);
    }
}
