using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace VeeamTest.Blocks
{

    public class DecompressingBlockAction : IBlockHandlingAction
    {
        private readonly Hasher.Hasher hasher;
        public DecompressingBlockAction(Hasher.Hasher hasher)
        {
            this.hasher = hasher;
        }

        public void Act(Block block)
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
            byte[] hash = this.hasher.GetHash(block.Data);

            var hashString = Hasher.Hasher.ToString(hash);
            Console.WriteLine("Block id = {0}   Hash = {1}", block.Id, hashString);

            if(!block.Hash.SequenceEqual(hash))
            {
                throw new InvalidDataException();
            }
        }
    }
}
