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
        public byte[] CompressedData { get; set; }


        public static Block ReadBlock(BinaryReader reader, int hashSize)
        {
            Block block = null;

            int blockSize = reader.ReadInt32();
            int blockId = reader.ReadInt32();

            byte[] hashBytes = reader.ReadBytes(hashSize);
            byte[] data = reader.ReadBytes(blockSize - hashSize - 4);

            block = new Block
            {
                Id = blockId,
                Hash = hashBytes,
                CompressedData = data
            };

            return block;
        }

        public static Block ReadBlock(Stream stream, int hashSize)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            int blockSize = BitConverter.ToInt32(bytes, 0);
            if (blockSize < hashSize + 4)
                throw new ArgumentOutOfRangeException();

            byte[] buffer = new byte[blockSize];
            stream.Read(buffer, 0, blockSize);

            int blockId = BitConverter.ToInt32(buffer, 0);
            int originBlockSize = BitConverter.ToInt32(buffer, 4);
            byte[] hash = new byte[hashSize];
            Array.Copy(buffer, 8, hash, 0, hash.Length);

            byte[] compressedData = new byte[blockSize - 8 - hashSize];
            Array.Copy(buffer, 8 + hashSize, compressedData, 0, compressedData.Length);

            Block block = new Block
            {
                Id = blockId,
                OriginBlockSize = originBlockSize,
                Hash = hash,
                CompressedData = compressedData
            };

            return block;
        }

        public static Block ReadBlock(BinaryReader reader, int hashSize, bool b)
        {
            byte[] bytes = new byte[4];
            reader.Read(bytes, 0, bytes.Length);
            int blockSize = BitConverter.ToInt32(bytes, 0);
            if (blockSize < hashSize + 4)
                throw new ArgumentOutOfRangeException();

            byte[] buffer = new byte[blockSize];
            reader.Read(buffer, 0, blockSize);

            int blockId = BitConverter.ToInt32(buffer, 0);
            int originBlockSize = BitConverter.ToInt32(buffer, 4);
            byte[] hash = new byte[hashSize];
            Array.Copy(buffer, 8, hash, 0, hash.Length);

            byte[] compressedData = new byte[blockSize - 8 - hashSize];
            Array.Copy(buffer, 8 + hashSize, compressedData, 0, compressedData.Length);

            Block block = new Block
            {
                Id = blockId,
                OriginBlockSize = originBlockSize,
                Hash = hash,
                CompressedData = compressedData
            };

            return block;
        }

        public byte[] ToCompressedByteArray()
        {
            byte[] idBytes = BitConverter.GetBytes(this.Id);
            byte[] originSize = BitConverter.GetBytes(this.OriginBlockSize);
            int blockSize = idBytes.Length + originSize.Length + this.Hash.Length + this.CompressedData.Length;
            byte[] blockSizeBytes = BitConverter.GetBytes(blockSize);

            byte[] result = new byte[blockSize + blockSizeBytes.Length];

            Array.Copy(blockSizeBytes, result, blockSizeBytes.Length);
            Array.Copy(idBytes, 0, result, blockSizeBytes.Length, idBytes.Length);
            Array.Copy(originSize, 0, result, blockSizeBytes.Length + idBytes.Length, originSize.Length);
            Array.Copy(this.Hash, 0, result, blockSizeBytes.Length + idBytes.Length + originSize.Length, this.Hash.Length);
            Array.Copy(this.CompressedData, 0, result, blockSizeBytes.Length + idBytes.Length + originSize.Length + Hash.Length, this.CompressedData.Length);

            return result;
        }
    }
}
