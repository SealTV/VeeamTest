using System.IO;
using System;

namespace VeeamTest.Blocks
{
    public class Block
    {
        public uint Id { get; set; }
        public int OriginBlockSize { get; set; }
        public byte[] Hash { get; set; }
        public byte[] Data { get; set; }

        public byte[] ToByteArray()
        {
            byte[] idBytes = BitConverter.GetBytes(this.Id);
            byte[] originSize = BitConverter.GetBytes(this.OriginBlockSize);

            int blockSize = idBytes.Length + originSize.Length + this.Hash.Length + this.Data.Length;
            byte[] blockSizeBytes = BitConverter.GetBytes(blockSize);

            byte[] result = new byte[blockSize + 4];

            Array.Copy(blockSizeBytes, result, blockSizeBytes.Length);
            Array.Copy(idBytes, 0, result, blockSizeBytes.Length, idBytes.Length);
            Array.Copy(originSize, 0, result, blockSizeBytes.Length + idBytes.Length, originSize.Length);
            Array.Copy(this.Hash, 0, result, blockSizeBytes.Length + idBytes.Length + originSize.Length, this.Hash.Length);
            Array.Copy(this.Data, 0, result, blockSizeBytes.Length + idBytes.Length + originSize.Length + Hash.Length, this.Data.Length);
            return result;
        }
    }
}
