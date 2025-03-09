/*using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlosTest.Threading
{
    public class ThreadPool<ResultType>
    {
        private static readonly int DefaultThreadCount = Environment.ProcessorCount;
        public int AvailableThreadCount { get; private set; }
        public int MaxThreadCount { get; private set; }
        private volatile bool Shutdown;

        private readonly List<Thread> _threads = new List<Thread>();
        private readonly Flos.Container.BlockingQueue<Action> _workQueue = new Flos.Container.BlockingQueue<Action>();

        public ThreadPool() : this(Environment.ProcessorCount) { }

        public ThreadPool(int availableThreadCount)
        {
            if (availableThreadCount < 1)
                throw new ArgumentException("AvailableThreadCount must be greater than 0");

            AvailableThreadCount = availableThreadCount;
            MaxThreadCount = availableThreadCount;
        }

        private void Worker()
        {
            while (true)
            {
                Action work = _workQueue.Dequeue();
                if (work == null) 
                    break;
                work();
            }
        }


        public void Start()
        {
            for (int i = 0; i < AvailableThreadCount; ++i)
            {
                var thread = new Thread(Worker)
                {
                    IsBackground = true // 后台线程不会阻止进程退出
                };
                thread.Start();
                _threads.Add(thread);
            }
        }

        public Task<ResultType> QueueTask(Func<ResultType> task)
        {
            if (Shutdown)
                throw new InvalidOperationException("ThreadPool is shutdown");

            var tcs = new TaskCompletionSource<ResultType>();
            _workQueue.Enqueue(() =>
            {
                try
                {
                    var result = task();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public void Shutdown()
        {
            Shutdown = true;

            // 添加毒丸任务（null）来终止每个工作线程
            foreach (var _ in _threads)
                _workQueue.Enqueue(null);

            // 等待所有线程完成
            foreach (var thread in _threads)
                thread.Join();

            _threads.Clear();
        }
    }
}

*/