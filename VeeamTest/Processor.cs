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

        private Thread producerTread;
        private Thread consumerThread;

        private EncoderServiceProvider encoderServiceProvider;

        private HashTypes hashType;

        private IStreamReader streamReader;
        private IStreamWriter streamWriter;

        private int blockSize;
        private bool isRun;
        private bool isAborder;

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

        // Run compress process 
        public bool RunCompress(int blockSize)
        {
            //this.encoderServiceProvider = new Encoder(Environment.ProcessorCount, this.hashType, s =>
            this.encoderServiceProvider = new Encoder(1, this.hashType, s =>
            {
                Console.WriteLine(s);
                this.Abort();
            });

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

            uint blocksCount = (uint)Math.Ceiling(this.inputStream.Length / (this.blockSize + 0.0));
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

            this.Run();
            return !isAborder;
        }

        // Run decompress process
        public bool RunDecompress()
        {
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

            this.streamReader = new CryptedStreamReader(this.inputStream);
            Header header;
            try
            {
                header = ((CryptedStreamReader)this.streamReader).GetHeader();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                if (this.outputStream != null)
                {
                    this.outputStream.Close();
                    this.outputStream.Dispose();

                    File.Delete(this.outputFile);
                }
                return false;
            }

            this.blockSize = header.BlockSize;
            this.hashType = header.HashType;

            if (this.hashType == HashTypes.Undefined)
            {
                return false;
            }

            this.encoderServiceProvider = new Decoder(Environment.ProcessorCount, this.hashType, s =>
            {
                Console.WriteLine(s);
                this.Abort();
            }); 

            this.streamWriter = new DefaultStreamWriter(this.outputStream, this.blockSize);

            this.Run();
            return !isAborder;
        }

        // Start threads and wait when all thread were stoped.
        private void Run()
        {
            this.producerTread = new Thread(Produce);
            this.producerTread.Priority = ThreadPriority.AboveNormal;

            this.consumerThread = new Thread(Consume);
            this.consumerThread.Priority = ThreadPriority.AboveNormal;

            this.producerTread.Start();
            this.encoderServiceProvider.Start();

            this.isRun = true;
            this.consumerThread.Start();
            this.producerTread.Join();
            this.encoderServiceProvider.Stop();
            this.isRun = false;
            this.consumerThread.Join();
        }

        // Read blocks from input stream and insert into service
        private void Produce()
        {
            long streamLength = this.inputStream.Length;
     
            while(streamLength - 1 > this.inputStream.Position)
            {
                Block nextBlock;
                try
                {
                    nextBlock = this.streamReader.GetNextBlock();
                }
                catch (ArgumentOutOfRangeException e)
                {
                    this.isAborder = true;
                    return;
                }

                while(!this.encoderServiceProvider.TryAddBlock(nextBlock))
                {
                    try
                    {
                        Thread.Sleep(10);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        return;
                    }
                }
            }

            this.inputStream.Close();
            this.inputStream.Dispose();
        }

        // Get blocks from service and write data into output stream.
        private void Consume()
        {
            List<Block> blocks;
            do
            {
                if(this.encoderServiceProvider.TryGetAvailableBlocks(out blocks))
                {
                    for(int i = 0; i < blocks.Count; i++)
                    {
                        this.streamWriter.WriteBlock(blocks[i]);
                        this.outputStream.Flush();
                    }
                }
                else
                {
                    try
                    {
                        Thread.Sleep(10);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        return;
                    }
                }

            } while(this.isRun);

            // Try get from buffer lasted blocks.
            if(this.encoderServiceProvider.TryGetAvailableBlocks(out blocks))
                for(int i = 0; i < blocks.Count; i++)
                {
                    this.streamWriter.WriteBlock(blocks[i]);
                    this.outputStream.Flush();
                }
            this.outputStream.Close();
            this.outputStream.Dispose();
        }

        public void Abort()
        {
            this.isAborder = true;
            if (this.producerTread != null)
            {
                this.producerTread.Interrupt();
                this.producerTread.Join();
            }

            if (this.consumerThread != null)
            {
                this.consumerThread.Interrupt();
                this.consumerThread.Join();
            }
           
            if (this.encoderServiceProvider != null)
            {
                this.encoderServiceProvider.Cancel();
                Console.WriteLine("Service canceled.");
            }

            if(this.outputStream != null)
            {
                this.outputStream.Close();
                this.outputStream.Dispose();

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

                throw new FileLoadException("Can't load file.", this.outputFile);
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
            return str == "yes" || str == "y";
        }
    }
}
