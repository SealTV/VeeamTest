using System;
    
using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public class Encoder : CompressorServiceProvider
    {
        public Encoder(int threadsCount, HashTypes hashType) : base(threadsCount, hashType)
        {}

        protected override void Act(Block block, Hasher.Hasher hasher)
        {
            byte[] hash = hasher.GetHash(block.OriginData);
            var hashString = Hasher.Hasher.ToString(hash);
            block.Hash = hash;
            Console.WriteLine("Block id = {0}   Hash = {1}", block.Id, hashString);
        }
    }
}
