using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;

using VeeamTest.Hasher;
using VeeamTest.Blocks;
using VeeamTest.StreamIO;


namespace VeeamTest
{
    internal class Processor
    {
        private readonly string inputFile;
        private readonly string outputFile;

        private Stream inputStream;
        private Stream outputStream;

        private Thread readTread;
        private Thread writeThread;

        private CompressorServiceProvider compressorServiceProvider;

        private HashTypes hashType;

        private IStreamReader streamReader;
        private IStreamWriter streamWriter;

        public Processor(string inputFile, string outputFile, HashTypes hashType)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.hashType = hashType;
        }

        public Processor(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.hashType = HashTypes.Undefined;
        }

        private int blockSize;
        public void RunCompress(int blockSize)
        {
            this.compressorServiceProvider = new Encoder(System.Environment.ProcessorCount, this.hashType);
            this.inputStream = this.GetInputStream();

            int blocksCount = (int)Math.Ceiling(this.inputStream.Length / (this.blockSize + 0.0));
            this.blockSize = blockSize;

            this.streamReader = new DefaultStreamReader(this.inputStream, blockSize);
            
            this.outputStream = this.GetOutputStream();
            this.streamWriter = new CryptedStreamWriter(this.outputStream);
            Header header = new Header
            {
                BlockSize = this.blockSize,
                BlocksCount = blocksCount,
                HashType = this.hashType,
                // TODO: change hash size
                HashSize = 16
            };

            (this.streamWriter as CryptedStreamWriter).WriteHeader(header);

            this.Run();
        }


        public void RunDecompress()
        {
            //this.compressorServiceProvider = new Decoder(System.Environment.ProcessorCount, this.hashType);
            this.compressorServiceProvider = new Decoder(1, this.hashType);
            this.inputStream = this.GetInputStream();
            Header header = Header.ReadHead(this.inputStream);
            this.blockSize = header.BlockSize;
            this.hashType = header.HashType;

            this.streamReader = new CryptedStreamReader(this.inputStream, header.HashSize);
            
            this.outputStream = this.GetOutputStream();
            this.streamWriter = new DefaultStreamWriter(this.outputStream, this.blockSize);

            this.Run();
        }

        private bool isRun;
        private void Run()
        {
            this.readTread = new Thread(Read);
            this.writeThread = new Thread(Write);
         
            this.readTread.Start();
            this.compressorServiceProvider.Start();

            this.isRun = true;
            this.writeThread.Start();

            this.readTread.Join();
            Console.WriteLine("End reading.");
            this.compressorServiceProvider.Stop();

            this.isRun = false;
            this.writeThread.Join();
            Console.WriteLine("End writing.");
        }

        private void Read()
        {
            long streamLength = this.inputStream.Length;
            using (var reader = new BinaryReader(this.inputStream))
            {
                while (streamLength - 1 > this.inputStream.Position)
                {
                    var nextBlock = this.streamReader.GetNextBlock(reader);
                    bool b;
                    do
                    {
                        b = this.compressorServiceProvider.AddBlock(nextBlock);
                        if (!b)
                            Thread.Sleep(100);
                    } while (!b);
                }
            }
            //while (streamLength - 1 > this.inputStream.Position)
            //{
            //    var nextBlock = this.streamReader.GetNextBlock();
            //    bool b;
            //    do
            //    {
            //        b = this.compressorServiceProvider.AddBlock(nextBlock);
            //        if (!b)
            //            Thread.Sleep(100);
            //    } while (!b);
            //}

            this.streamReader = null;
            this.inputStream.Dispose();
            this.inputStream = null;
        }

        private void Write()
        {
            //while (this.isRun)
            //{
            //    List<Block> blocks;
            //    if (this.compressorServiceProvider.TryGetAvailableBlocks(out blocks))
            //    {
            //        for (int i = 0; i < blocks.Count; i++)
            //        {
            //            this.streamWriter.WriteBlock(blocks[i]);
            //            blocks[i] = null;
            //            this.outputStream.Flush();
            //        }
            //    }
            //    else
            //    {
            //        Thread.Sleep(100);
            //    }
            //}

            using (var writer = new BinaryWriter(this.outputStream))
            {
                while (this.isRun)
                {
                    List<Block> blocks;
                    if (this.compressorServiceProvider.TryGetAvailableBlocks(out blocks))
                    {
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            this.streamWriter.WriteBlock(blocks[i], writer);
                            blocks[i] = null;
                            this.outputStream.Flush();
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }

            this.streamWriter = null;
            this.outputStream.Dispose();
            this.outputStream = null;
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
                Console.WriteLine("Reading canceled.");
            }

            try
            {
                if(this.writeThread != null)
                    this.writeThread.Abort();
            }
            catch(ThreadAbortException e)
            {
                Console.WriteLine("Writing canceled.");
            }

            if (this.compressorServiceProvider != null)
            {
                this.compressorServiceProvider.Cancel();
                Console.WriteLine("Service canceled.");
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

            return File.Open(this.outputFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        private bool GetConsoleResult()
        {
            string str;
            do
            {
                str = Console.ReadLine();
                if (str != null)
                    str = str.ToLower();
            }
            while (str != "yes" && str != "y" && str != "no" && str != "n");

            if(str == "yes" || str == "y")
            {
                return true;
            }

            return false;
        }
    }
}
