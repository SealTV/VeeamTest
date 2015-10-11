using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public class Header
    {
        public int BlockSize { get; set; }
        public uint BlocksCount { get; set; }
        public HashTypes HashType { get; set; }
        public int HashSize { get; set; }
    }
}
