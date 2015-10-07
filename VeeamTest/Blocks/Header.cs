using System;
using System.IO;
using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public class Header
    {
        public int BlockSize { get; set; }
        public uint BlocksCount { get; set; }
        public HashTypes HashType { get; set; }
        public int HashSize { get; set; }

        public byte[] ToByteArray()
        {
            byte[] blockSizeBytes = BitConverter.GetBytes(this.BlockSize);
            byte[] blocksCountBytes = BitConverter.GetBytes(this.BlocksCount);
            byte hashTypeByte = (byte)this.HashType;
            byte[] hashSizeBytes = BitConverter.GetBytes(this.HashSize);

            byte[] result = new byte[13];
            Array.Copy(blockSizeBytes, result, 4);
            Array.Copy(blocksCountBytes, 0, result, 4, 4);
            result[8] = hashTypeByte;
             Array.Copy(hashSizeBytes, 0, result, 9, 4);

            return result;
        }
    }
}
