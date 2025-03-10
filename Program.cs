using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch sw = new ();
            Console.WriteLine(Environment.ProcessorCount);
            sw.Start();
            for (int i = 0; i < 40; i++)
            {
                int index = i;
                Flos.Threading.ThreadPool.Run(() => HeavyJob(index));
            }
            
            Flos.Threading.ThreadPool.WhenAll();
            sw.Stop();
            Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
        }


        public static void HeavyJob(int i)
        {
            Thread.Sleep(5000);
            Console.WriteLine($" Thread {Environment.CurrentManagedThreadId} - Job {i} Done");
        }
    }
}