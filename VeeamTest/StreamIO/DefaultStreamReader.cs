using System;
using System.IO;
using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    public class DefaultStreamReader : IStreamReader
    {
        private readonly Stream stream;

        private readonly int blockSize;
        private uint counter;

        public DefaultStreamReader(Stream stream, int blockSize)
        {
            this.stream = stream;
            this.blockSize = blockSize;
        }

        public byte[] GetNextBlockBytes()
        {
            byte[] buffer = new byte[this.blockSize];
            int bytesReaded = this.stream.Read(buffer, 0, this.blockSize);

            if (bytesReaded == 0)
                return null;

            if(bytesReaded == this.blockSize)
                return buffer;

            byte[] result = new byte[bytesReaded];
            Array.Copy(buffer, result, result.Length);

            return result;
        }

        public Block GetNextBlock()
        {
            var buffer = this.GetNextBlockBytes();
            Block block = new Block
            {
                Id = this.counter++,
                OriginBlockSize = buffer.Length,
                Data = buffer
            };

            return block;
        }
    }
}
