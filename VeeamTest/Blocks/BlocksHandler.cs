using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public class BlocksHandler
    {
        private readonly object inpitLockObject;
        private readonly object outputLockObject;
        private readonly Queue<Block> input;
        private readonly Queue<Block> output;

        private readonly Thread[] workers;

        private readonly HashTypes hashType;

        private const int MAX_QUEUE_SIZE = 30;
        private event Action<string> collback;

        private readonly EventWaitHandle addUnhandledBlockWaitHandler = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle getUnhandledBlockWaitHandler = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle addHandledBlockWaitHandler = new EventWaitHandle(false, EventResetMode.AutoReset);

        /// <summary>
        /// </summary>
        /// <param name="threadsCount">Count of workers.</param>
        /// <param name="hashType">Type of hash.</param>
        /// <param name="operationType"></param>
        /// <param name="collback"></param>
        public BlocksHandler(int threadsCount, HashTypes hashType, OperationType operationType,  Action<string> collback)
        {
            this.inpitLockObject = new object();
            this.outputLockObject = new object();

            this.input = new Queue<Block>();
            this.output = new Queue<Block>();

            this.workers = new Thread[threadsCount];

            for (int i = 0; i < threadsCount; i++)
            {
                this.workers[i] = new Thread(()=> Run(operationType));
                this.workers[i].Priority = ThreadPriority.Lowest;
            }

            this.hashType = hashType;
            this.collback = collback;
        }

        public void Start()
        {
            for(int i = 0; i < this.workers.Length; i++)
            {
                this.workers[i].IsBackground = true;
                this.workers[i].Start();
            }
            
            Console.WriteLine("Started {0} workers.", this.workers.Count());
        }

        public void Stop()
        {
            lock (this.inpitLockObject)
            {
                for(int i = 0; i < this.workers.Length; i++)
                    this.input.Enqueue(null);
            }

            foreach (var w in this.workers)
            {
                w.Join();
                Console.WriteLine("Worker stoped!");
            }
        }

        private void Run(OperationType operationType)
        {
            var hasher = Hasher.Hasher.GetHasher(hashType);
            IBlockHandlingAction action = operationType == OperationType.Compress
                                          ? (IBlockHandlingAction)(new CompressingBlockAction(hasher))
                                          : (IBlockHandlingAction)(new DecompressingBlockAction(hasher));
            while(true)
            {
                Block block = null;
                try
                {
                    lock (this.inpitLockObject)
                    {
                        if(this.input.Count > 0)
                        {
                            block = this.input.Dequeue();
                            // Stoping work
                            if(block == null)
                                return;
                        }
                    }
                    this.addUnhandledBlockWaitHandler.Set();

                    if(block != null)
                    {
                        action.Act(block);
                        this.EnqueueHandledBlock(block);
                    }
                    else
                    {
                        this.getUnhandledBlockWaitHandler.WaitOne();
                    }
                }
                catch(ThreadAbortException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
                catch(Exception e)
                {
                    if(collback != null)
                    {
                        collback(e.Message);
                    }
                    return;
                }
            }
        }

        public void Abort()
        {
            for(int i = 0; i < this.workers.Length; i++)
            {
                this.workers[i].Abort();
            }
        }

        public void AddUnhandledBlock(Block block)
        {
            bool isFullQueue;
            lock (this.inpitLockObject)
            {
                isFullQueue = this.input.Count >= MAX_QUEUE_SIZE;
            }

            if(isFullQueue)
                this.addUnhandledBlockWaitHandler.WaitOne();

            lock (this.inpitLockObject)
            {
                this.input.Enqueue(block);
            }

            this.getUnhandledBlockWaitHandler.Set();
        }

        public List<Block> GetAvailableBlocks()
        {
            List<Block> result;
            lock (this.outputLockObject)
            {
                result = this.output.ToList();
                this.output.Clear();
            }

            this.addHandledBlockWaitHandler.Set();
            return result;
        }

        private void EnqueueHandledBlock(Block block)
        {
            bool isFullQueue;
            lock (this.outputLockObject)
            {
                isFullQueue = this.output.Count >= MAX_QUEUE_SIZE;
            }

            if(isFullQueue)
                this.addHandledBlockWaitHandler.WaitOne();

            lock (this.outputLockObject)
            {  
                this.output.Enqueue(block);
            }
        }
    }
}
