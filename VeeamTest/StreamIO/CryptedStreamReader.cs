using System.IO;
using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    public class CryptedStreamReader : IStreamReader
    {
        private readonly Stream stream;
        private readonly int hashSize;
        public CryptedStreamReader(Stream stream, int hashSize)
        {
            this.stream = stream;
            this.hashSize = hashSize;
        }
        
        public Block GetNextBlock()
        {
            var block = Block.ReadBlock(this.stream, this.hashSize);
            return block;
        }
    }
}
