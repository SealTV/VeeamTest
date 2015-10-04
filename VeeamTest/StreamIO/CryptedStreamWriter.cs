﻿using System.IO;
using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    public class CryptedStreamWriter : IStreamWriter
    {
        private Stream stream;

        public CryptedStreamWriter(Stream stream)
        {
            this.stream = stream;
        }

        public void WriteHeader(Header header)
        {
            var buffer = header.ToCompresedByteArray();
            this.stream.Write(buffer, 0, buffer.Length);
        }

        public void WriteBlock(Block block)
        {
            var buffer = block.ToCompresedByteArray();
            this.stream.Write(buffer, 0, buffer.Length);
        }

    }
}
