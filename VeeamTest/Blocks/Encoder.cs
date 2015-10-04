using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using VeeamTest.Hasher;

namespace VeeamTest.Blocks
{
    public class Encoder
    {
        private object inpitLockObjcet;
        private object outputLockObjcet;
        private Queue<Block> input;
        private Queue<Block> output;
        private int counter;

        private Thread[] workers;

        private HashTypes hashType;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadsCount">Count of workers.</param>
        public Encoder(int threadsCount, HashTypes hashType)
        {
            this.inpitLockObjcet = new object();
            this.outputLockObjcet = new object();

            this.input = new Queue<Block>();
            this.output = new Queue<Block>();

            this.workers = new Thread[threadsCount];
            for(int i = 0; i < threadsCount; i++)
            {
                this.workers[i] = new Thread(Run);
            }

            this.hashType = hashType;
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
                this.AddBlock(null);

            foreach(var w in this.workers)
            {
                w.Join();
                Console.WriteLine("Worker Stoped!");
            }
        }

        private void Run()
        {
            Hasher.Hasher hasher = Hasher.Hasher.GetHasher(hashType);
            Console.WriteLine("Worker started!");
            Block inputBytes = null;
            do
            {
                lock (this.inpitLockObjcet)
                {
                    while(this.input.Count == 0)
                        Monitor.Wait(this.inpitLockObjcet);
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

                lock (this.outputLockObjcet)
                {
                    this.output.Enqueue(inputBytes);
                    Monitor.PulseAll(this.outputLockObjcet);
                }
            } while(inputBytes != null);
        }
        

        public void AddBlock(Block block)
        {
            lock (inpitLockObjcet)
            {
                block.Id = counter++;
                this.input.Enqueue(block);
                Monitor.PulseAll(this.inpitLockObjcet);
            }
        }

        public Block[] GetAvailableBlocks()
        {
            Block[] result = null;
            lock (this.outputLockObjcet)
            {
                while(this.output.Count == 0)
                    Monitor.Wait(this.outputLockObjcet);

                result = this.output.ToArray();
                this.output.Clear();
            }

            return result;
        }
    }
}
