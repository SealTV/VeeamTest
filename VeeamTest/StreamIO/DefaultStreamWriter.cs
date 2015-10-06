using System.IO;
using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{

    public class DefaultStreamWriter : IStreamWriter
    {
        private readonly Stream stream;
        private readonly int blockSize;

        public DefaultStreamWriter(Stream stream, int blockSize)
        {
            this.stream = stream;
            this.blockSize = blockSize;
            this.stream.Flush();
        }

        public void WriteBlock(Block block)
        {
            WriteBlock(block.Data, block.Id);
        }

        public void WriteBlock(byte[] data, long id)
        {
            this.stream.Position = this.blockSize * id;
            this.stream.Write(data, 0, data.Length);
        }
    }
}
