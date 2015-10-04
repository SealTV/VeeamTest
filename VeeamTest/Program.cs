using System;

namespace VeeamTest
{
    public class Program
    {
        private static Processor proccessor;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Handler);
            
            //System.Threading.Thread t = new System.Threading.Thread(() => { while(true) Console.WriteLine("run"); });
            //t.Start();
            //while(true)
            //{
            //    string str = Console.ReadLine();
            //    Console.WriteLine(str);
            //}
            
            Options options = new Options();
            CommandLine.Parser.Default.ParseArguments(args, options);

            if(options.IsCompress == options.IsDecompress)
            {
                Console.WriteLine(options.GetUsage());
                Console.WriteLine(0);
                return;
            }

            if(options.IsCompress)
            {
                // TODO: Compress file
                proccessor = new Processor(options.InputFileName, options.OutputFileName, options.HashType);
                proccessor.RunComperss();
                return;
            }

            if(options.IsDecompress)
            {
                // TODO: Decompress file
                proccessor = new Processor(options.InputFileName, options.OutputFileName, options.HashType);
                proccessor.RunDecompress();
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