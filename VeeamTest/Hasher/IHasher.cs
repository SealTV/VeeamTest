namespace VeeamTest.Hasher
{
    interface IHasher
    {
        string GetHash(byte[] input, out byte[] output);
        bool VerifyHash(byte[] input, byte[] hash);
    }
}
