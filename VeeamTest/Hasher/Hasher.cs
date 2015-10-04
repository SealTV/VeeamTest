namespace VeeamTest.Hasher
{
    public abstract class Hasher
    {
        public static Hasher GetHasher(HashTypes hashType)
        {
            switch(hashType)
            {
                case HashTypes.SHA256:
                    throw new System.ArgumentNullException();
                case HashTypes.MD5:
                    return new MD5Hasher();
                default:
                    return new MD5Hasher();
            }
        }

        public abstract string GetHash(byte[] input, out byte[] output);
        public abstract bool VerifyHash(byte[] input, byte[] hash);
    }
}
