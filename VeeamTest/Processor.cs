using System;
using System.IO;
using System.Threading;
using VeeamTest.Blocks;
using VeeamTest.StreamIO;

namespace VeeamTest
{
    internal class Processor
    {
        private string inputFile;
        private string outputFile;

        private Stream inputStream;
        private Stream outputStream;

        private Thread readTread;
        private Thread writeThread;

        private Encoder encoder;
        private Decoder decoder;

        private Hasher.HashTypes hashType;

        private IStreamReader streamReader;
        private IStreamWriter streamWriter;

        public Processor(string inputFile, string outputFile, Hasher.HashTypes hashType)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.hashType = hashType;
        }

        public void RunComperss()
        {
            this.encoder = new Encoder(System.Environment.ProcessorCount, this.hashType);

            this.encoder.Start();

            this.readTread = new Thread(Read);
            this.writeThread = new Thread(Write);

            this.readTread.Join();
            this.writeThread.Join();
        }


        public void RunDecompress()
        {
            this.encoder = new Encoder(System.Environment.ProcessorCount, this.hashType);

            this.encoder.Start();

            this.readTread = new Thread(Read);
            this.writeThread = new Thread(Write);

            this.readTread.Join();
            this.writeThread.Join();
        }

        private void Read()
        {
            this.inputStream = this.GetInputStream();
            
        }


        private void Write()
        {
            this.outputStream = this.GetOutputStream();
        }

        public void Abort()
        {
            try
            {
                if(this.readTread != null)
                    this.readTread.Abort();
            }
            catch(ThreadAbortException e)
            {

            }

            try
            {
                if(this.writeThread != null)
                    this.writeThread.Abort();
            }
            catch(ThreadAbortException e)
            {

            }

            if(this.encoder != null)
            {
                this.encoder.Stop();
            }

            if(this.decoder != null)
            {
                this.decoder.Stop();
            }
        }

        private Stream GetInputStream()
        {
            if(!File.Exists(this.inputFile))
            {
                throw new FileNotFoundException("File not found.", this.inputFile);
            }

            return File.Open(this.inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private Stream GetOutputStream()
        {
            if(File.Exists(this.outputFile))
            {
                Console.WriteLine("File {0} already exist. Override? (y/n)", this.outputStream);
                bool isOverride = this.GetConsoleResult();
                if(isOverride)
                {
                    return File.Open(this.outputFile, FileMode.Truncate, FileAccess.Write, FileShare.Read);
                }
                else
                {
                    throw new FileLoadException();
                }
            }

            return File.Open(this.outputFile, FileMode.Open, FileAccess.Write, FileShare.Read);
        }

        private bool GetConsoleResult()
        {
            string str;
            do
            {
                str = Console.ReadLine().ToLower();
            } while(str != "yes" || str != "y" || str != "no" || str != "n");

            if(str == "yes" || str == "y")
            {
                return true;
            }

            return false;
        }
    }
}
