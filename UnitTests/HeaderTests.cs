using NUnit.Framework;
using System.IO;
using VeeamTest.Blocks;
using VeeamTest.Hasher;

namespace UnitTests
{
    using VeeamTest.StreamIO;

    [TestFixture]
    public class HeaderTests
    {
        [Test]
        public void GetByteArrayTest()
        {
            // Arrage
            Header header = new Header
            {
                BlocksCount = 10,
                BlockSize = 1024,
                HashSize = 16,
                HashType = HashTypes.MD5
            };

            // Act
            MemoryStream stream = new MemoryStream();
            BlockStreamWriter streamWriter = new BlockStreamWriter(stream);
            streamWriter.WriteHeader(header);
            var buffer = stream.ToArray();
            // Assert
            Assert.AreEqual(buffer.Length, 13);
        }

        [Test]
        public void GetBlockFromStreamTest()
        {
            // Arrage
            var buffer = new byte[] { 0, 4, 0, 0, 10, 0, 0, 0, 0, 16, 0, 0, 0 };

            // Act
            BlockStreamReader streamReader = new BlockStreamReader(new MemoryStream(buffer));
            Header header = streamReader.GetHeader();

            // Assert
            Assert.AreEqual(header.BlocksCount, 10);
            Assert.AreEqual(header.BlockSize, 1024);
            Assert.AreEqual(header.HashSize, 16);
            Assert.AreEqual(header.HashType, HashTypes.MD5);
        }
    }
}
