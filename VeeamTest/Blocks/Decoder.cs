using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

using VeeamTest.Hasher;
using System.Threading;
using System;
using System.Linq;

namespace VeeamTest.Blocks
{
    public class Decoder
    {
        private object inpitLockObjcet;
        private object outputLockObjcet;
        private Queue<Block> input;
        private Queue<Block> output;

        private Thread[] workers;

        private HashTypes hashType;
        
        /// <summary>
        /// </summary>
        /// <param name="threadsCount">Count of workers.</param>
        /// <param name="hashType">Type of hash.</param>
        public Decoder(int threadsCount, HashTypes hashType)
        {
            this.inpitLockObjcet = new object();
            this.outputLockObjcet = new object();

            this.input = new Queue<Block>();
            this.output = new Queue<Block>();

            this.workers = new Thread[threadsCount];
            for(int i = 0; i< threadsCount; i++)
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
            Hasher.Hasher hasher = Hasher.Hasher.GetHasher(this.hashType);
            ;
            Console.WriteLine("Worker started!");
            Block compresedBlock = null;
            do
            {
                lock (this.inpitLockObjcet)
                {
                    while(this.input.Count == 0)
                        Monitor.Wait(this.inpitLockObjcet);
                    compresedBlock = this.input.Dequeue();
                }

                if(compresedBlock == null)
                    return;

                using(var zipStream = new GZipStream(new MemoryStream(), CompressionMode.Decompress))
                {

                    zipStream.Write(compresedBlock.CompresedData, 0, compresedBlock.CompresedData.Length);
                    byte[] originData = ((MemoryStream)zipStream.BaseStream).ToArray();

                    compresedBlock.OriginData = originData;
                    byte[] hash;
                    hasher.GetHash(compresedBlock.OriginData, out hash);
                    if(!Enumerable.SequenceEqual(compresedBlock.Hash, hash))
                    {
                        throw new InvalidDataException();
                    }
                }

                lock (this.outputLockObjcet)
                {
                    this.output.Enqueue(compresedBlock);
                    Monitor.PulseAll(this.outputLockObjcet);
                }

            } while(compresedBlock != null);
        }

        public void AddBlock(Block block)
        {
            lock (inpitLockObjcet)
            {
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
