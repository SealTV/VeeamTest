using System;
using System.IO;
using System.IO.Compression;

namespace VeeamTest.Blocks
{
    public class CompressingBlockAction : IBlockHandlingAction
    {
        private readonly Hasher.Hasher hasher;
        public CompressingBlockAction(Hasher.Hasher hasher)
        {
            this.hasher = hasher;
        }

        public void Act(Block block)
        {
            byte[] hash = this.hasher.GetHash(block.Data);
            var hashString = Hasher.Hasher.ToString(hash);
            block.Hash = hash;
            Console.WriteLine("Block id = {0}   Hash = {1}", block.Id, hashString);

            using(var stream = new MemoryStream())
            {
                using(var compressStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    compressStream.Write(block.Data, 0, block.OriginBlockSize);
                }
                block.Data = stream.ToArray();
            }
        }
    }
}
