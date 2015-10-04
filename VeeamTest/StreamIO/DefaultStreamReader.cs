using System;
using System.IO;
using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    public  class DefaultStreamReader : IStreamReader
    {
        private Stream stream;

        private int blockSize;

        public DefaultStreamReader(Stream stream, int blockSize)
        {
            this.stream = stream;
            this.blockSize = blockSize;
        }


        public byte[] GetNextBlockBytes()
        {
            byte[] buffer = new byte[this.blockSize];
            int bytesReadet = this.stream.Read(buffer, 0, this.blockSize);

            if(bytesReadet == this.blockSize)
                return buffer;

            byte[] result = new byte[bytesReadet];
            Array.Copy(buffer, result, result.Length);

            return result;
        }

        public Block GetNextBlock()
        {
            Block block = new Block();
          
            block.OriginData = this.GetNextBlockBytes();
            return block;
        }
    }
}
