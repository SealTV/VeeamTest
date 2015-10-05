using System;

namespace VeeamTest
{
    public class Program
    {
        private static Processor proccessor;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Handler);

            Options options = new Options();
            CommandLine.Parser.Default.ParseArguments(args, options);

            if(options.IsCompress == options.IsDecompress
                || string.IsNullOrEmpty(options.InputFileName) || string.IsNullOrEmpty(options.OutputFileName))
            {
                Console.WriteLine(0);
                return;
            }

            if(options.IsCompress)
            {
                // note: Compress file
                proccessor = new Processor(options.InputFileName, options.OutputFileName, options.HashType);
                var result = proccessor.RunCompress(options.Blocksize);
                Console.WriteLine(result ? 1 : 0);
                return;
            }

            if(options.IsDecompress)
            {
                // note: Decompress file
                proccessor = new Processor(options.InputFileName, options.OutputFileName, options.HashType);
                var result = proccessor.RunDecompress();
                Console.WriteLine(result ? 1 : 0);
                return;
            }
        }

        private static void Handler(object sender, ConsoleCancelEventArgs args)
        {
            if(proccessor != null)
            {
                proccessor.Abort();
            }

            Console.WriteLine("Canceled.");
            Console.WriteLine(0);

            args.Cancel = false;
        }
    }
}