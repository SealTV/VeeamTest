using System;
using System.IO;
using System.Linq;

using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public class Decoder : CompressorServiceProvider
    {
        public Decoder(int threadsCount, HashTypes hashType) : base(threadsCount, hashType)
        {}

        protected override void Act(Block block, Hasher.Hasher hasher)
        {
            byte[] hash = hasher.GetHash(block.OriginData);

            var hashString = Hasher.Hasher.ToString(hash);
            Console.WriteLine("Block id = {0}   Hash = {1}", block.Id, hashString);

            if(!block.Hash.SequenceEqual(hash))
            {
                throw new InvalidDataException();
            }
        }
    }
}
