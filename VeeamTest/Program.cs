using System;
using System.Collections.Generic;
using System.Text;
using VeeamTest.Blocks;

namespace VeeamTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Queue<byte[]> queue = new Queue<byte[]>();
            queue.Enqueue(Encoding.Unicode.GetBytes("Hello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            Encryptor encryprtor = new Encryptor(4, 100);
            //encryprtor.Run();
            encryprtor.Start();
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello worldHello dHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello worsaasdasddasldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello wosadasrldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHellasdao wdssasdasdorldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello worldHelasfgagalo worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello wasdasorldHadgagello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello worldaadsasdasdadHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello worasdasldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello wordsadasdasddldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello woasdasdrldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));
            encryprtor.AddBinaryBlock(Encoding.Unicode.GetBytes("Hello worldHello worsadaldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world"));

            Console.ReadKey();
            encryprtor.Stop();
            Console.ReadKey();
        }
    }
}
