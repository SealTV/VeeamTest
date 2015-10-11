using VeeamTest.Blocks;

using System.IO;
using NUnit.Framework;

namespace UnitTests
{
    using VeeamTest.StreamIO;

    [TestFixture]
    public class BlockTests
    {
        [Test]
        public void GetByteArrayTest()
        {
            // Arrage
            Block block = new Block
            {
                Id = 1,
                OriginBlockSize = 1,
                Hash = new byte[] { 1, 2, 3, 4 },
                Data = new byte[] { 1, 2, 3, 4 }
            };
            
            // Act
            MemoryStream stream = new MemoryStream();
            BlockStreamWriter streamWriter = new BlockStreamWriter(stream);
            streamWriter.WriteBlock(block);
            var buffer = stream.ToArray();

            // Assert
            Assert.AreEqual(buffer.Length, 20);
        }

        [Test]
        public void GetBlockFromStreamTest()
        {
            // Arrage
            var buffer = new byte[] {16, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 2, 3, 4, 1, 2, 3, 4};

            // Act
            BlockStreamReader streamReader = new BlockStreamReader(new MemoryStream(buffer), 4);
            Block block = streamReader.GetNextBlock();

            // Assert
            Assert.AreEqual(block.Id, 1);
            Assert.AreEqual(block.OriginBlockSize, 1);
            Assert.AreEqual(block.Hash, new byte[] { 1, 2, 3, 4 });
            Assert.AreEqual(block.Data, new byte[] { 1, 2, 3, 4 });
        }
    }
}
