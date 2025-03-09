using System;
using System.Threading;
using System.Threading.Tasks;

namespace Program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Flos.Threading.ThreadPool threadPool = new(3);

            threadPool.Start();

            for (int i = 0; i < 15; i++)
            {
                threadPool.AddTask(() => HeavyJob());
            }
            
            Thread.Sleep(6000);
            threadPool.Shutdown();

        }


        public static void HeavyJob()
        {
            Thread.Sleep(1000);
            Console.WriteLine($"{Environment.CurrentManagedThreadId} - Heavy Job Done");
        }
    }
}