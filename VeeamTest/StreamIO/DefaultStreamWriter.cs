using System.IO;
using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    public class DefaultStreamWriter : IStreamWriter
    {
        private Stream stream;
        private int blockSize;

        public DefaultStreamWriter(Stream stream, int blockSize)
        {
            this.stream = stream;
            this.blockSize = blockSize;
        }

        public void WriteBlock(Block block)
        {
            this.stream.Position = this.blockSize * block.Id;
            this.stream.Write(block.OriginData, 0, block.OriginData.Length);
        }
    }
}
