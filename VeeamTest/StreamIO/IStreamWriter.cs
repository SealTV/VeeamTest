using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    using System.IO;

    public interface IStreamWriter
    {
        void WriteBlock(Block block);

        void WriteBlock(Block block, BinaryWriter writer);
    }
}
