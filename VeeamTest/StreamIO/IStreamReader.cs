using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    using System.IO;

    public interface IStreamReader
    {
        Block GetNextBlock();
        Block GetNextBlock(BinaryReader reader);
    }
}
