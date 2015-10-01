using System;
using System.IO;
using System.IO.Compression;
using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{

    public class Encoder : EncoderServiceProvider
    {
        public Encoder(int threadsCount, HashTypes hashType, Action<string> collback)
            : base(threadsCount, hashType, collback)
        { }

        protected override void Act(Block block, Hasher.Hasher hasher)
        {
            byte[] hash = hasher.GetHash(block.Data);
            var hashString = Hasher.Hasher.ToString(hash);
            block.Hash = hash;
            Console.WriteLine("Block id = {0}   Hash = {1}", block.Id, hashString);
        
            using (var stream = new MemoryStream())
            {
                using (var compressStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    compressStream.Write(block.Data, 0, block.OriginBlockSize);
                }

                block.Data = stream.ToArray();
            }

        }
    }
}
