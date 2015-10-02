using System.IO;
using System;

namespace VeeamTest.Blocks
{
    public class Block
    {
        public int Id { get; set; }
        public byte[] Hash { get; set; }
        public byte[] OriginData { get; set; }
        public byte[] CompresedData { get; set; }


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
                CompresedData = data
            };

            return block;
        }

        public static Block ReadBlock(Stream stream, int hashSize)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            int blockSize = BitConverter.ToInt32(bytes, 0);
            if(blockSize < hashSize + 4)
                throw new ArgumentOutOfRangeException();

            byte[] buffer = new byte[blockSize];
            stream.Read(buffer, 0, blockSize);

            int blockId = BitConverter.ToInt32(buffer, 0);
            byte[] hash = new byte[hashSize];
            Array.Copy(buffer, 4, hash, 0, hash.Length);

            byte[] data = new byte[blockSize - 4 - hashSize];
            Array.Copy(buffer, 4 + hashSize, data, 0, data.Length);

            Block block = new Block()
            {
                Id = blockId,
                Hash = hash,
                CompresedData = data
            };

            return block;
        }

        public byte[] ToCompresedByteArray()
        {
            byte[] idBytes = BitConverter.GetBytes(this.Id);

            int blockSize = idBytes.Length + this.Hash.Length + this.CompresedData.Length;
            byte[] blockSizeBytes = BitConverter.GetBytes(blockSize);

            byte[] result = new byte[blockSize + blockSizeBytes.Length];

            Array.Copy(blockSizeBytes, result, blockSizeBytes.Length);
            Array.Copy(idBytes, 0, result, blockSizeBytes.Length, idBytes.Length);
            Array.Copy(this.Hash, 0, result, blockSizeBytes.Length + idBytes.Length, this.Hash.Length);
            Array.Copy(this.CompresedData, 0, result, blockSizeBytes.Length + idBytes.Length + Hash.Length, this.CompresedData.Length);

            return result;
        }
    }
}
