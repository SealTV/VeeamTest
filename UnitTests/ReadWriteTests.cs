using  NUnit.Framework;

namespace UnitTests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using VeeamTest.Blocks;
    using VeeamTest.Hasher;
    using VeeamTest.StreamIO;

    [TestFixture]
    public class ReadWriteTests
    {
        [Test]
        public void ReadWriteCompressedTest()
        {
            // Assert 
            byte[] originBytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            Encoder encoder = new Encoder(1, HashTypes.MD5);
            DefaultStreamReader dReader = new DefaultStreamReader(new MemoryStream(originBytes), 11);

            Header h1 = new Header()
            {
                BlocksCount = 3,
                BlockSize = 11,
                HashSize = 16,
                HashType = HashTypes.MD5
            };
           
            var b1 = dReader.GetNextBlock();
            var b2 = dReader.GetNextBlock();
            var b3 = dReader.GetNextBlock();

            encoder.AddBlock(b1);
            encoder.AddBlock(b2);
            encoder.AddBlock(b3);

            encoder.Start();

            encoder.Stop();

            List<Block> array = (List<Block>)encoder.GetAvailableBlocks();

            MemoryStream bufferStream = new MemoryStream();
            CryptedStreamWriter cryptedStreamWriter = new CryptedStreamWriter(bufferStream);
            cryptedStreamWriter.WriteHeader(h1);

            for(int i = 0; i < array.Count(); i++)
            {
                cryptedStreamWriter.WriteBlock(array[i]);
            }

            bufferStream.Seek(0, SeekOrigin.Begin);

            Header h2 = Header.ReadHead(bufferStream);
            CryptedStreamReader cryptedStreamReader = new CryptedStreamReader(bufferStream, 16);
            var b11 = cryptedStreamReader.GetNextBlock();
            var b21 = cryptedStreamReader.GetNextBlock();
            var b31 = cryptedStreamReader.GetNextBlock();

            Decoder decoder = new Decoder(1, HashTypes.MD5);

            decoder.AddBlock(b11);
            decoder.AddBlock(b21);
            decoder.AddBlock(b31);

            decoder.Start();
            decoder.Stop();

            List<Block> array2 = (List<Block>)decoder.GetAvailableBlocks();

            Assert.AreEqual(array2.Count(), 3);

            Assert.AreEqual(h1.BlocksCount, h2.BlocksCount);
            Assert.AreEqual(h1.BlockSize, h2.BlockSize);
            Assert.AreEqual(h1.HashSize, h2.HashSize);
            Assert.AreEqual(h1.HashType, h2.HashType);

            Assert.AreEqual(b1.Id, b11.Id);
            Assert.AreEqual(b1.OriginBlockSize, b11.OriginBlockSize);
            Assert.AreEqual(b1.OriginData, b11.OriginData);
            Assert.AreEqual(b1.Hash, b11.Hash);

            Assert.AreEqual(b2.Id, b21.Id);
            Assert.AreEqual(b2.OriginBlockSize, b21.OriginBlockSize);
            Assert.AreEqual(b2.OriginData, b21.OriginData);
            Assert.AreEqual(b2.Hash, b21.Hash);

            Assert.AreEqual(b3.Id, b31.Id);
            Assert.AreEqual(b3.OriginBlockSize, b31.OriginBlockSize);
            Assert.AreEqual(b3.OriginData, b31.OriginData);
            Assert.AreEqual(b3.Hash, b31.Hash);
        }
    }
}
