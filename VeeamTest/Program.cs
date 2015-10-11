using System;
using System.IO;
using VeeamTest.Processor;

namespace VeeamTest
{

    public class Program
    {
        public static void Main(string[] args)
        {
            Options options = new Options();
            CommandLine.Parser.Default.ParseArguments(args, options);

            if(options.IsCompress == options.IsDecompress || string.IsNullOrEmpty(options.InputFileName) || string.IsNullOrEmpty(options.OutputFileName))
            {
                Console.WriteLine(0);
                return;
            }

            Program program = new Program();
            Console.CancelKeyPress += program.Handler;

            program.RunConsole(options);
        }

        private BaseProcessor baseProcessor;
        private Stream inputStream;
        private Stream outputStream;

        private void RunConsole(Options options)
        {
            try
            {
                inputStream = GetInputStream(options.InputFileName);
                outputStream = GetOutputStream(options.OutputFileName);
            }
            catch(FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(0);
                return;
            }
            catch(FileLoadException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(0);
                return;
            }

            this.baseProcessor = null;
            if(options.IsCompress)
            {
                this.baseProcessor = new CompressionProcessor(inputStream, outputStream, options.Blocksize, options.HashType);
            }
            else
            {
                this.baseProcessor = new DecompressionProcessor(inputStream, outputStream);
            }

            var result = this.baseProcessor.Run();
            Console.WriteLine(result ? 1 : 0);

            this.DisposeStreams();
        }

        private void Handler(object sender, ConsoleCancelEventArgs args)
        {
            if (this.baseProcessor != null)
            {
                this.baseProcessor.Abort();
            }

            args.Cancel = true;
        }


        private Stream GetInputStream(string inputFile)
        {
            if(!File.Exists(inputFile))
            {
                throw new FileNotFoundException("File not found.", inputFile);
            }

            return File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private Stream GetOutputStream(string outputFile)
        {
            if(File.Exists(outputFile))
            {
                Console.WriteLine("File {0} already exist. Override? (y/n)", outputFile);
                bool isOverride = GetConsoleResult();
                if(isOverride)
                {
                    return File.Open(outputFile, FileMode.Truncate, FileAccess.Write, FileShare.Read);
                }

                throw new FileLoadException("Can't load file.", outputFile);
            }

            return File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        private void DisposeStreams()
        {
            if(inputStream != null)
            {
                inputStream.Close();
                inputStream.Dispose();
            }

            if(outputStream != null)
            {
                outputStream.Close();
                outputStream.Dispose();
            }
        }

        private static bool GetConsoleResult()
        {
            string str;
            do
            {
                str = Console.ReadLine();
                if(str != null)
                    str = str.ToLower();
            }
            while(str != "yes" && str != "y" && str != "no" && str != "n");
            return str == "yes" || str == "y";
        }
    }
}