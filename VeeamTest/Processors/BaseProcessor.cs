using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using VeeamTest.Hasher;
using VeeamTest.Blocks;
using VeeamTest.StreamIO;


namespace VeeamTest.Processor
{
    internal abstract class BaseProcessor
    {
        protected int blockSize;
        protected readonly Stream inputStream;
        protected readonly Stream outputStream;

        private BlocksHandler blocksHandler;

        protected HashTypes hashType;
        private readonly OperationType operationType;

        protected IStreamReader streamReader;
        protected IStreamWriter streamWriter;

        private bool isRun;
        private bool isAborder;
        private Thread producerTread;
        private Thread consumerThread;

        protected BaseProcessor(Stream inputStream, Stream outputStream, OperationType operationType, int blockSize, HashTypes hashType = HashTypes.Undefined)
        {
            this.hashType = hashType;
            this.inputStream = inputStream;
            this.outputStream = outputStream;
            this.operationType = operationType;
            this.blockSize = blockSize;
        }

        protected BaseProcessor(Stream inputStream, Stream outputStream, OperationType operationType, HashTypes hashType = HashTypes.Undefined)
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

        protected abstract bool Init();
      
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
                    Console.WriteLine("Block reading fail. Reading of input file are stoped.{0}", e.Message);
                    return;
                }
                catch (ThreadAbortException e)
                {
                    Console.WriteLine(e.Message);
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

            if (this.blocksHandler != null)
            {
                this.blocksHandler.Abort();
            }

            if (this.consumerThread != null)
            {
                this.consumerThread.Abort();
            }
        }
    }
}
