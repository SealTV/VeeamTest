using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

using VeeamTest.Hasher;
using System.Threading;
using System;

namespace VeeamTest.Blocks
{
    public class Encroding
    {
        private object lockObj1;
        private object lockObj2;
        private Queue<Block> input;
        private Queue<Block> output;


        private Thread[] workers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadsCount">Count of workers.</param>
        public Encroding(int threadsCount)
        {
            this.lockObj1 = new object();
            this.lockObj2 = new object();

            this.input = new Queue<Block>();
            this.output = new Queue<Block>();

            this.workers = new Thread[threadsCount];
            for(int i = 0; i< threadsCount; i++)
            {
                this.workers[i] = new Thread(Run);
            }
        }


        public void Start()
        {
            foreach(var w in this.workers)
            {
                w.Start();
            }
        }

        public void Stop()
        {
            for(int i = 0; i < this.workers.Length; i++)
                this.AddBinaryBlock(null);

            foreach(var w in this.workers)
            {
                w.Join();
                Console.WriteLine("Worker Stoped!");
            }
        }


        private void Run()
        {
            IHasher hasher = new MD5Hasher();
            Console.WriteLine("Worker started!");
            Block inputBytes = null;
            do
            {
                lock (this.lockObj1)
                {
                    while(this.input.Count == 0)
                        Monitor.Wait(this.lockObj1);
                    inputBytes = this.input.Dequeue();
                }

                if(inputBytes == null)
                    continue;

                using(var zipStream = new GZipStream(new MemoryStream(), CompressionMode.Compress))
                {
                    byte[] hash;
                    string str = hasher.GetHash(inputBytes.OriginData, out hash);
                    Console.WriteLine("Item: {0} Hash: {1}", inputBytes.Id, str);

                    zipStream.Write(inputBytes.OriginData, 0, inputBytes.OriginData.Length);

                    byte[] output = ((MemoryStream)zipStream.BaseStream).ToArray();

                    inputBytes.Hash = hash;
                    inputBytes.CompresedData = output;
                }

                lock (this.lockObj2)
                {
                    this.output.Enqueue(inputBytes);
                    Monitor.PulseAll(this.lockObj2);
                }
            } while(inputBytes != null);
        }

        private int counter;
        public void AddBinaryBlock(byte[] block)
        {
            lock (lockObj1)
            {
                this.input.Enqueue(block == null? null : new Block { OriginData = block, Id = counter++ });
                Monitor.PulseAll(this.lockObj1);
            }
        }

        public Block[] GetAvailableBlocks()
        {
            Block[] result = null;
            lock (this.lockObj2)
            {
                while(this.output.Count == 0)
                    Monitor.Wait(this.lockObj2);

                result = this.output.ToArray();
                this.output.Clear();             
            }

            return result;
        }
    }
}
