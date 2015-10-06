using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public abstract class EncoderServiceProvider
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
        protected EncoderServiceProvider(int threadsCount, HashTypes hashType, Action<string> collback)
        {
            this.inpitLockObject = new object();
            this.outputLockObject = new object();

            this.input = new Queue<Block>();
            this.output = new Queue<Block>();

            this.workers = new Thread[threadsCount];
            for (int i = 0; i < threadsCount; i++)
            {
                this.workers[i] = new Thread(Run);
            }

            this.hashType = hashType;
            this.collback = collback;
        }

        public void Start()
        {
            foreach (var worker in this.workers)
            {
                worker.IsBackground = true;
                worker.Start();
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

        public bool TryAddBlock(Block block)
        {
            lock (this.inpitLockObject)
            {
                bool result = false;
                if(this.input.Count < MAX_QUEUE_SIZE)
                {
                    this.input.Enqueue(block);
                    result = true;
                }

                Monitor.PulseAll(this.inpitLockObject);
                return result;
            }
        }

        public bool TryGetAvailableBlocks(out List<Block> blocks)
        {
            lock (this.outputLockObject)
            {
                if (this.output.Count == 0)
                {
                    blocks = null;
                    return false;
                }

                blocks = this.output.ToList();
                this.output.Clear();
            }

            return true;
        }

        public IEnumerable<Block> GetAvailableBlocks()
        {
            List<Block> result;
            lock (this.outputLockObject)
            {
                while (this.output.Count == 0)
                    Monitor.Wait(this.outputLockObject);

                result = this.output.ToList();
                this.output.Clear();
            }

            return result;
        }

        public void Cancel()
        {
            foreach (var worker in this.workers)
            {
                try
                {
                    worker.Abort();                    
                }
                catch (ThreadAbortException e)
                {
                    Console.WriteLine("Worker aborted.");
                }
            }    
        }

        private void Run()
        {
            var hasher = Hasher.Hasher.GetHasher(hashType);

            Block block;
            do
            {
                lock (this.inpitLockObject)
                {
                    while (this.input.Count == 0)
                        Monitor.Wait(this.inpitLockObject);

                    block = this.input.Dequeue();
                }

                if (block == null)
                    continue;

                try
                {
                    this.Act(block, hasher);
                }
                catch (Exception e)
                {
                    if (collback != null)
                    {
                        collback(e.Message);
                    }
                }
                this.EnqueueBlock(block);

            } while (block != null);
        }

        private void EnqueueBlock(Block block)
        {
            bool isQueued = false;
            do
            {
                lock (this.outputLockObject)
                {
                    if (this.output.Count <= MAX_QUEUE_SIZE)
                    {
                        this.output.Enqueue(block);
                        Monitor.PulseAll(this.outputLockObject);
                        isQueued = true;
                    }
                }
                if (!isQueued)
                    Thread.Sleep(10);
            }
            while (!isQueued);
        }

        protected abstract void Act(Block block, Hasher.Hasher hasher);
    }
}
