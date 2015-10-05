using VeeamTest.Blocks;

namespace VeeamTest.StreamIO
{
    public interface IStreamWriter
    {
        void WriteBlock(Block block);
    }
}
