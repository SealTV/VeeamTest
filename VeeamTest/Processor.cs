using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

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

        private int blockSize;
        private bool isRun;

        public Processor(string inputFile, string outputFile, HashTypes hashType)
        {
            this.hashType = hashType;
            this.inputFile = inputFile;
            this.outputFile = outputFile;
        }

        public Processor(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.hashType = HashTypes.Undefined;
        }

        public bool RunCompress(int blockSize)
        {
            this.compressorServiceProvider = new Encoder(Environment.ProcessorCount, this.hashType);

            try
            {
                this.inputStream = this.GetInputStream();
                this.outputStream = this.GetOutputStream();
            }
            catch(FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            catch(FileLoadException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            int blocksCount = (int)Math.Ceiling(this.inputStream.Length / (this.blockSize + 0.0));
            this.blockSize = blockSize;

            this.streamReader = new DefaultStreamReader(this.inputStream, blockSize);

            this.streamWriter = new CryptedStreamWriter(this.outputStream);
            Header header = new Header
            {
                BlockSize = this.blockSize,
                BlocksCount = blocksCount,
                HashType = this.hashType,
                HashSize = Hasher.Hasher.GetHashSize(this.hashType)
            };

            (this.streamWriter as CryptedStreamWriter).WriteHeader(header);

            return this.Run();
        }

        public bool RunDecompress()
        {
            this.compressorServiceProvider = new Decoder(Environment.ProcessorCount, this.hashType);
            try
            {
                this.inputStream = this.GetInputStream();
                this.outputStream = this.GetOutputStream();
            } catch(FileNotFoundException e)
            {
                Console.WriteLine("{0} {1} ",e.Message, e.FileName);
                return false;
            } catch(FileLoadException e)
            {
                Console.WriteLine("{0} {1} ",e.Message, e.FileName);
                return false; 
            }

            Header header;
            try
            {
                header = Header.ReadHead(this.inputStream);
            }catch(ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                if(this.outputStream != null)
                {
                    this.outputStream.Close();
                    File.Delete(this.outputFile);
                }
                return false;
            }

            this.blockSize = header.BlockSize;
            this.hashType = header.HashType;

            if(this.hashType == HashTypes.Undefined)
            {
                return false;
            }

            this.streamReader = new CryptedStreamReader(this.inputStream, header.HashSize);
            this.streamWriter = new DefaultStreamWriter(this.outputStream, this.blockSize);

            return this.Run();
        }

        private bool Run()
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

            return true;
        }

        private void Read()
        {
            long streamLength = this.inputStream.Length;
     
            while(streamLength - 1 > this.inputStream.Position)
            {
                var nextBlock = this.streamReader.GetNextBlock();

                while(!this.compressorServiceProvider.AddBlock(nextBlock))
                {
                    Thread.Sleep(10);
                }
            }

            this.inputStream.Close();
            this.inputStream.Dispose();
        }

        private void Write()
        {
            List<Block> blocks;
            do
            {
                if(this.compressorServiceProvider.TryGetAvailableBlocks(out blocks))
                {
                    for(int i = 0; i < blocks.Count; i++)
                    {
                        this.streamWriter.WriteBlock(blocks[i]);
                        this.outputStream.Flush();
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }

            } while(this.isRun);

            if(this.compressorServiceProvider.TryGetAvailableBlocks(out blocks))
            {
                for(int i = 0; i < blocks.Count; i++)
                {
                    this.streamWriter.WriteBlock(blocks[i]);
                    this.outputStream.Flush();
                }
            }
            this.outputStream.Close();
            this.outputStream.Dispose();
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

            if(this.outputStream != null)
            {
                this.outputStream.Close();
                File.Delete(this.outputFile);
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
                    throw new FileLoadException("Can't load file.", this.outputFile);
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
