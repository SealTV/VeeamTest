using System.IO;

using NUnit.Core;
using NUnit.Framework;
using VeeamTest.StreamIO;

namespace UnitTests
{
    [TestFixture]
    public class DefaultStreamReaderTest
    {
        [Test]
        public void GetNextBlockTesting()
        {
            // Arrage 
            byte[] bytes = new byte[]
            {
                1, 2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20
            };

            var fileReader = new DefaultStreamReader(new MemoryStream(bytes), 9);

            // Act
            byte[] b1 = fileReader.GetNextBlockBytes();
            byte[] b2 = fileReader.GetNextBlockBytes();
            byte[] b3 = fileReader.GetNextBlockBytes();

            // Assert
            Assert.AreEqual(b1.Length, 9);
            Assert.AreEqual(b2.Length, 9);
            Assert.AreEqual(b3.Length, 2);
        }
    }
}
