using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using VeeamTest.Hasher;
using VeeamTest.Blocks;
using VeeamTest.StreamIO;


namespace VeeamTest
{
    internal abstract class Processor
    {
        protected readonly string inputFile;
        protected readonly string outputFile;

        protected int blockSize;
        protected Stream inputStream;
        protected Stream outputStream;
       
        protected BlocksHandler blocksHandler;

        protected HashTypes hashType;
        protected OperationType operationType;

        protected IStreamReader streamReader;
        protected IStreamWriter streamWriter;

        protected bool isRun;
        protected bool isAborder;
        private Thread producerTread;
        private Thread consumerThread;

        protected Processor(Stream inputStream, Stream outputStream, OperationType operationType, int blockSize, HashTypes hashType = HashTypes.Undefined)
        {
            this.hashType = hashType;
            this.inputStream = inputStream;
            this.outputStream = outputStream;
            this.operationType = operationType;
            this.blockSize = blockSize;
        }

        protected Processor(Stream inputStream, Stream outputStream, OperationType operationType, HashTypes hashType = HashTypes.Undefined)
        {
            this.hashType = hashType;
            this.inputStream = inputStream;
            this.outputStream = outputStream;
            this.operationType = operationType;
        }

        public bool Run()
        {
            this.blocksHandler = new BlocksHandler(Environment.ProcessorCount, this.hashType, this.operationType, s =>
            {
                Console.WriteLine(s);
                this.Abort();
            });

            if(!this.Init())
                return false;

            this.Process(); 

            return !isAborder;
        }

        public abstract bool Init();
      
        // Start threads and wait when all thread were stoped.
        private void Process()
        {
            this.producerTread = new Thread(Produce);
            this.producerTread.Priority = ThreadPriority.AboveNormal;

            this.consumerThread = new Thread(Consume);
            this.consumerThread.Priority = ThreadPriority.AboveNormal;

            this.producerTread.Start();
            this.blocksHandler.Start();

            this.isRun = true;
            this.consumerThread.Start();
            this.producerTread.Join();
            this.blocksHandler.Stop();
            this.isRun = false;
            this.consumerThread.Join();
        }

        // Read blocks from input stream and insert into service
        private void Produce()
        {
            long streamLength = this.inputStream.Length;
     
            while(streamLength - 1 > this.inputStream.Position)
            {
                try
                {
                    Block nextBlock = this.streamReader.GetNextBlock();
                    this.blocksHandler.AddUnhandledBlock(nextBlock);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    this.isAborder = true;
                    return;
                }
            }
        }

        // Get blocks from service and write data into output stream.
        private void Consume()
        {
            List<Block> blocks;
            do
            {
                blocks = this.blocksHandler.GetAvailableBlocks();
                for(int i = 0; i < blocks.Count; i++)
                {
                    this.streamWriter.WriteBlock(blocks[i]);
                    this.outputStream.Flush();
                }

            } while(this.isRun);

            blocks = this.blocksHandler.GetAvailableBlocks();
            for(int i = 0; i < blocks.Count; i++)
            {
                this.streamWriter.WriteBlock(blocks[i]);
                this.outputStream.Flush();
            }
        }

        public void Abort()
        {
            this.isAborder = true;
            if (this.producerTread != null)
            {
                this.producerTread.Abort();
            }

            if (this.consumerThread != null)
            {
                this.producerTread.Abort();
            }
           
            if (this.blocksHandler != null)
            {
                this.blocksHandler.Cancel();
                Console.WriteLine("Block handler Stoped.");
            }
        }
    }
}
