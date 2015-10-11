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


        /// <summary>
        /// </summary>
        /// <param name="threadsCount">Count of workers.</param>
        /// <param name="hashType">Type of hash.</param>
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
            for (int i = 0; i < this.workers.Length; i++)
            {
                lock (this.inpitLockObject)
                {
                    this.input.Enqueue(null);
                    Monitor.PulseAll(this.inpitLockObject);
                }
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
            Block block = null;
            do
            {
                try
                {
                    lock (this.inpitLockObject)
                    {
                        while (this.input.Count == 0)
                            Monitor.Wait(this.inpitLockObject);

                        block = this.input.Dequeue();
                        Monitor.PulseAll(this.inpitLockObject);
                    }

                    if (block == null)
                        continue;

                    action.Act(block);

                    this.EnqueueHandledBlock(block);
                }
                catch (ThreadAbortException e)
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
            } while(block != null);
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
            lock (this.inpitLockObject)
            {
                while(this.input.Count >= MAX_QUEUE_SIZE)
                {
                    Monitor.Wait(this.inpitLockObject);
                }

                this.input.Enqueue(block);
                Monitor.PulseAll(this.inpitLockObject);
            }
        }

        public List<Block> GetAvailableBlocks()
        {
            List<Block> result;
            lock (this.outputLockObject)
            {
                result = this.output.ToList();
                this.output.Clear();

                Monitor.PulseAll(this.outputLockObject);
            }

            return result;
        }

        private void EnqueueHandledBlock(Block block)
        {
            lock (this.outputLockObject)
            {
                while(this.output.Count >= MAX_QUEUE_SIZE)
                {
                    Monitor.Wait(this.outputLockObject);
                }

                this.output.Enqueue(block);
                Monitor.PulseAll(this.outputLockObject);
            }
        }

    }
}
