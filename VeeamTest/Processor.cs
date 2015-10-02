using System.Threading;
using VeeamTest.Blocks;

namespace VeeamTest
{
    public class Processor
    {
        private string inputFile;
        private string outputFile;
        private Encroding entriptor;

        public Processor(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;

            this.entriptor = new Encroding(System.Environment.ProcessorCount);
        }

        public void Run()
        {
            this.entriptor.Start();

            Thread t1 = new Thread(Read);
            Thread t2 = new Thread(Write);


        }


        private void Read()
        { }


        private void Write()
        { }
    
    }
}
