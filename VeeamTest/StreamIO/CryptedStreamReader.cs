using System.IO;
using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    public class CryptedStreamReader : IStreamReader
    {
        private Stream stream;
        private readonly int hashSize;

        public CryptedStreamReader(Stream stream, int hashSize)
        {
            this.stream = stream;
            this.hashSize = hashSize;
        }

        public Header GetHeader()
        {
            var header = Header.ReadHead(this.stream);
            return header;
        }

        public Block GetNextBlock()
        {
            var block = Block.ReadBlock(this.stream, this.hashSize);
            return block;
        }
    }
}
