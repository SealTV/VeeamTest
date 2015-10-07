using System;
using System.IO;
using VeeamTest.Blocks;
using VeeamTest.Hasher;
using VeeamTest.StreamIO;

namespace VeeamTest
{
    internal class DecompressionProcessor : Processor
    {
        public DecompressionProcessor(Stream inputStream, Stream outputStream)
            : base(inputStream, outputStream, OperationType.Decompress)
        { }

        public override bool Init()
        {
            this.streamReader = new BlockStreamReader(this.inputStream);

            Header header;
            try
            {
                header = ((BlockStreamReader)this.streamReader).GetHeader();
            }
            catch(ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                if(this.outputStream != null)
                {
                    this.outputStream.Close();
                    this.outputStream.Dispose();

                    File.Delete(this.outputFile);
                }
                return false;
            }

            this.blockSize = header.BlockSize;
            this.hashType = header.HashType;

            if(this.hashType == HashTypes.Undefined)
            {
                return false;
            }

            this.streamWriter = new DefaultStreamWriter(this.outputStream, this.blockSize);
            return true;
        }
    }
}
