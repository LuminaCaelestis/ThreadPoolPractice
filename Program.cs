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
            for (int i = 0; i < 50; i++)
            {
                int index = i;
                Flos.Threading.ThreadPool.Run(() => IncrementRes());
            }
            Console.WriteLine("All tasks pushed to queue");
            Flos.Threading.ThreadPool.WhenAll();
            Console.WriteLine("All tasks done");
            sw.Stop();
            Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
        }


        public static void HeavyJob(int i)
        {
            Thread.Sleep(5000);
            Console.WriteLine($" Thread {Environment.CurrentManagedThreadId} - Job {i} Done");
        }

        public static int res = 0;
        private static object _resLock = new();

        public static void IncrementRes()
        {
            lock(_resLock)
            {
                ++res;
                Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} done - res: {res}");
            }
        }
    }
}