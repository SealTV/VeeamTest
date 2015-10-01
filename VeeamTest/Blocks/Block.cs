namespace VeeamTest.Blocks
{
    public class Block
    {
        public int Id { get; set; }
        public byte[] Hash { get; set; }
        public byte[] OriginData { get; set; }
        public byte[] CompresedData { get; set; }

    }
}
