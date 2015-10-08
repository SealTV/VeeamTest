namespace VeeamTest.Blocks
{
    public class Block
    {
        public uint Id { get; set; }
        public int OriginBlockSize { get; set; }
        public byte[] Hash { get; set; }
        public byte[] Data { get; set; }
    }
}
