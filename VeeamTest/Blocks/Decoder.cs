using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{

    public class Decoder : EncoderServiceProvider
    {
        public Decoder(int threadsCount, HashTypes hashType, Action<string> collback)
            : base(threadsCount, hashType, collback)
        {}

        protected override void Act(Block block, Hasher.Hasher hasher)
        {

            byte[] data = new byte[block.OriginBlockSize];

            using (var stream = new MemoryStream(block.Data))
            {
                using (var compressStream = new GZipStream(stream, CompressionMode.Decompress, true))
                {
                    compressStream.Read(data, 0, block.OriginBlockSize);
                    stream.Close();
                    compressStream.Close();
                }

            }

            block.Data = data;

            byte[] hash = hasher.GetHash(block.Data);

            var hashString = Hasher.Hasher.ToString(hash);
            Console.WriteLine("Block id = {0}   Hash = {1}", block.Id, hashString);

            if(!block.Hash.SequenceEqual(hash))
            {
                throw new InvalidDataException();
            }
        }
    }
}
