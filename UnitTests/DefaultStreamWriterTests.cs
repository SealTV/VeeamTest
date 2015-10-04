using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VeeamTest.Blocks;
using VeeamTest.StreamIO;

namespace UnitTests
{
    [TestFixture]
    public class DefaultStreamWriterTests
    {
        [Test]
        public void WriteBlockTest()
        {
            // Arrage 
            MemoryStream stream = new MemoryStream();
            DefaultStreamWriter writer = new DefaultStreamWriter(stream, 4);

            Block block1 = new Block()
            {
                Id = 0,
                OriginData = new byte[] { 1, 1, 1, 1 }
            };

            Block block2 = new Block()
            {
                Id = 1,
                OriginData = new byte[] { 2, 2 }
            };

            // Act
            writer.WriteBlock(block1);
            writer.WriteBlock(block2);

            byte[] result = stream.ToArray();

            // Assert
            Assert.AreEqual(result.Length, 6);
            Assert.AreEqual(result, new byte[] { 1, 1, 1, 1, 2, 2 });
        }

        [Test]
        public void WriteBlockRandomOrderTest()
        {
            // Arrage 
            MemoryStream stream = new MemoryStream();
            DefaultStreamWriter writer = new DefaultStreamWriter(stream, 4);

            Block block1 = new Block()
            {
                Id = 0,
                OriginData = new byte[] { 1, 1, 1, 1 }
            };

            Block block2 = new Block()
            {
                Id = 1,
                OriginData = new byte[] { 2, 2, 2, 2 }
            };

            Block block3 = new Block()
            {
                Id = 2,
                OriginData = new byte[] { 3, 3 }
            };

            // Act
            writer.WriteBlock(block2);
            writer.WriteBlock(block3);
            writer.WriteBlock(block1);

            byte[] result = stream.ToArray();

            // Assert
            Assert.AreEqual(result.Length, 10);
            Assert.AreEqual(result, new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3});
        }
    }
}
