using System.IO;
using System;

namespace VeeamTest.Blocks
{
    public class Block
    {
        public int Id { get; set; }
        public int OriginBlockSize { get; set; }
        public byte[] Hash { get; set; }
        public byte[] OriginData { get; set; }

        public static Block ReadBlock(Stream stream, int hashSize)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            int blockSize = BitConverter.ToInt32(bytes, 0);
            Console.WriteLine("dada = " + (blockSize + 4));

            if (blockSize < hashSize + 4)
                throw new ArgumentOutOfRangeException();

            byte[] buffer = new byte[blockSize];
            stream.Read(buffer, 0, blockSize);

            int blockId = BitConverter.ToInt32(buffer, 0);
            int originBlockSize = BitConverter.ToInt32(buffer, 4);
            byte[] hash = new byte[hashSize];
            Array.Copy(buffer, 8, hash, 0, hash.Length);

            byte[] compressedData = new byte[originBlockSize];
            Array.Copy(buffer, 8 + hashSize, compressedData, 0, compressedData.Length);

            Block block = new Block
            {
                Id = blockId,
                OriginBlockSize = originBlockSize,
                Hash = hash,
                OriginData = compressedData
            };

            return block;
        }

        public byte[] ToCompressedByteArray()
        {
            byte[] idBytes = BitConverter.GetBytes(this.Id);
            byte[] originSize = BitConverter.GetBytes(this.OriginBlockSize);

            int blockSize = idBytes.Length + originSize.Length + this.Hash.Length + this.OriginData.Length;
            byte[] blockSizeBytes = BitConverter.GetBytes(blockSize);

            byte[] result = new byte[blockSize + 4];

            Array.Copy(blockSizeBytes, result, blockSizeBytes.Length);
            Array.Copy(idBytes, 0, result, blockSizeBytes.Length, idBytes.Length);
            Array.Copy(originSize, 0, result, blockSizeBytes.Length + idBytes.Length, originSize.Length);
            Array.Copy(this.Hash, 0, result, blockSizeBytes.Length + idBytes.Length + originSize.Length, this.Hash.Length);
            Array.Copy(this.OriginData, 0, result, blockSizeBytes.Length + idBytes.Length + originSize.Length + Hash.Length, this.OriginData.Length);
            return result;
        }
    }
}
