using System;
using System.IO;
using VeeamTest.Blocks;
using VeeamTest.Hasher;
using VeeamTest.StreamIO;

namespace VeeamTest
{
    internal class CompressionProcessor : Processor
    {
        public CompressionProcessor(Stream inputStream, Stream outputStream, int blockSize, HashTypes hashType = HashTypes.Undefined)
            : base(inputStream, outputStream, OperationType.Compress, blockSize, hashType)
        {}

        public override bool Init()
        {
            uint blocksCount = (uint)Math.Ceiling(this.inputStream.Length / (this.blockSize + 0.0));

            this.streamReader = new DefaultStreamReader(this.inputStream, this.blockSize);

            Header header = new Header
            {
                BlockSize = this.blockSize,
                BlocksCount = blocksCount,
                HashType = this.hashType,
                HashSize = Hasher.Hasher.GetHashSize(this.hashType)
            };

            this.streamWriter = new BlockStreamWriter(this.outputStream);
            (this.streamWriter as BlockStreamWriter).WriteHeader(header);
            return true;
        }
    }
}
