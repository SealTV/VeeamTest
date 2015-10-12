using System.IO;
using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    using System;
    using System.IO.Compression;
    using VeeamTest.Hasher;

    public class BlockStreamReader : IStreamReader
    {
        private readonly Stream stream;
        private int hashSize;

        public BlockStreamReader(Stream stream)
        {
            this.stream = stream;
            this.hashSize = 0;
        }

        public BlockStreamReader(Stream stream, int hastSize)
        {
            this.stream = stream;
            this.hashSize = hastSize;
        }

        public Header GetHeader()
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            int blockSize = BitConverter.ToInt32(bytes, 0);

            stream.Read(bytes, 0, bytes.Length);
            uint blocksCount = BitConverter.ToUInt32(bytes, 0);

            int hashType = stream.ReadByte();

            stream.Read(bytes, 0, bytes.Length);
            hashSize = BitConverter.ToInt32(bytes, 0);

            return new Header
            {
                BlockSize = blockSize,
                BlocksCount = blocksCount,
                HashType = (HashTypes)hashType,
                HashSize = hashSize
            };
        }

        public Block GetNextBlock()
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            int blockSize = BitConverter.ToInt32(bytes, 0);

            if (blockSize < hashSize + 4)
                throw new ArgumentOutOfRangeException();

            byte[] buffer = new byte[blockSize];
            stream.Read(buffer, 0, blockSize);

            uint blockId = BitConverter.ToUInt32(buffer, 0);
            int originBlockSize = BitConverter.ToInt32(buffer, 4);
            byte[] hash = new byte[hashSize];
            Array.Copy(buffer, 8, hash, 0, hash.Length);

            byte[] data = new byte[blockSize - 8 - hashSize];
            Array.Copy(buffer, 8 + hashSize, data, 0, data.Length);

            Block block = new Block
            {
                Id = blockId,
                OriginBlockSize = originBlockSize,
                Hash = hash,
                Data = data
            };
            return block;
        }
    }
}
