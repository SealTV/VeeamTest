using System;
using System.IO;
using System.IO.Compression;

using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public class Encoder : CompressorServiceProvider
    {
        public Encoder(int threadsCount, HashTypes hashType) : base(threadsCount, hashType)
        {}

        protected override void Act(Block block, Hasher.Hasher hasher)
        {
            byte[] hash;
            string str = hasher.GetHash(block.OriginData, out hash);
            Console.WriteLine("Item: {0} Hash: {1}", block.Id, str);
            MemoryStream stream = new MemoryStream();
            using (var zipStream = new GZipStream(stream, CompressionMode.Compress))
            {
                zipStream.Write(block.OriginData, 0, block.OriginData.Length);
            }

            block.Hash = hash;
            block.CompressedData = stream.ToArray();
        }
    }
}
