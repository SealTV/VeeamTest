using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    public interface IStreamReader
    {
        Block GetNextBlock();
    }
}
