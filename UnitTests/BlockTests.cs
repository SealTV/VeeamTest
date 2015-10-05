using VeeamTest.Blocks;

using System.IO;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class BlockTests
    {
        [Test]
        public void GetByteArrayTest()
        {
            // Arrage
            Block block = new Block()
            {
                Id = 1,
                Hash = new byte[] { 1, 2, 3, 4 },
                CompressedData = new byte[] { 1, 2, 3, 4 }
            };
            
            // Act
            var buffer = block.ToCompressedByteArray();

            // Assert
            Assert.AreEqual(buffer.Length, 16);
        }

        [Test]
        public void GetBlockFromStreamTest()
        {
            // Arrage
            var buffer = new byte[] {12, 0, 0, 0, 1, 0, 0, 0, 1, 2, 3, 4, 1, 2, 3, 4};

            // Act
            Block block = Block.ReadBlock(new MemoryStream(buffer), 4);

            // Assert
            Assert.AreEqual(block.Id, 1);
            Assert.AreEqual(block.Hash, new byte[] { 1, 2, 3, 4 });
            Assert.AreEqual(block.CompressedData, new byte[] { 1, 2, 3, 4 });
        }
    }
}
