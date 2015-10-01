using  NUnit.Framework;

namespace UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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

            Encoder encoder = new Encoder(1, HashTypes.MD5, Console.WriteLine);

            DefaultStreamReader dReader = new DefaultStreamReader(new MemoryStream(originBytes), 11);

            Header h1 = new Header
            {
                BlocksCount = 3,
                BlockSize = 11,
                HashSize = 16,
                HashType = HashTypes.MD5
            };
           
            var b1 = dReader.GetNextBlock();
            var b2 = dReader.GetNextBlock();
            var b3 = dReader.GetNextBlock();

            encoder.TryAddBlock(new Block()
            {
                Data = b1.Data,
                Hash = b1.Hash,
                OriginBlockSize = b1.OriginBlockSize,
                Id = b1.Id
            });
            encoder.TryAddBlock(new Block()
            {
                Data = b3.Data,
                Hash = b3.Hash,
                OriginBlockSize = b3.OriginBlockSize,
                Id = b3.Id
            });
            encoder.TryAddBlock(new Block()
            {
                Data = b2.Data,
                Hash = b2.Hash,
                OriginBlockSize = b2.OriginBlockSize,
                Id = b2.Id
            });

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

            CryptedStreamReader cryptedStreamReader = new CryptedStreamReader(bufferStream);
            Header h2 = cryptedStreamReader.GetHeader();
            var b11 = cryptedStreamReader.GetNextBlock();
            var b21 = cryptedStreamReader.GetNextBlock();
            var b31 = cryptedStreamReader.GetNextBlock();

            Decoder decoder = new Decoder(1, HashTypes.MD5, Console.WriteLine);

            decoder.TryAddBlock(b11);
            decoder.TryAddBlock(b21);
            decoder.TryAddBlock(b31);

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
            Assert.AreEqual(b1.Data, b11.Data);

            Assert.AreEqual(b3.Id, b21.Id);
            Assert.AreEqual(b3.OriginBlockSize, b21.OriginBlockSize);
            Assert.AreEqual(b3.Data, b21.Data);

            Assert.AreEqual(b2.Id, b31.Id);
            Assert.AreEqual(b2.OriginBlockSize, b31.OriginBlockSize);
            Assert.AreEqual(b2.Data, b31.Data);
        }
    }
}
