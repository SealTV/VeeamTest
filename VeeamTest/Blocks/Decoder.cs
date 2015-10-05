using System;
using System.IO;
using System.Linq;
using System.IO.Compression;

using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public class Decoder : CompressorServiceProvider
    {
        public Decoder(int threadsCount, HashTypes hashType) : base(threadsCount, hashType)
        {}

        protected override void Act(Block block, Hasher.Hasher hasher)
        {
            using (MemoryStream ms = new MemoryStream(block.CompressedData, true))
            {
                ms.Position = 0;
                using (var decompressionStream = new GZipStream(ms, CompressionMode.Decompress))
                {
                    byte[] originData = new byte[(int)ms.Length];
                    int length = decompressionStream.Read(originData, 0, originData.Length);
                   
                    block.OriginData = new byte[length];
                    Array.Copy(originData, block.OriginData, length);
                    byte[] hash;
                    Console.WriteLine("Block id = {0}   Hash = {1}", block.Id,
                                      hasher.GetHash(block.OriginData, out hash));
                    if (!block.Hash.SequenceEqual(hash))
                    {
                        throw new InvalidDataException();
                    }
                }
            }
        }
    }
}
