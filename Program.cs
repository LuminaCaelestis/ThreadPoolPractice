using System;
using System.Threading;
using System.Threading.Tasks;

namespace Program
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }


        public static void HeavyJob()
        {
            Thread.Sleep(1000);
            Console.WriteLine($"{Environment.CurrentManagedThreadId} - Done");
        }
    }
}