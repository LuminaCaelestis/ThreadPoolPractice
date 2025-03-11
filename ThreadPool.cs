using System;
using System.Collections.Generic;
using System.Threading;

namespace Flos.Threading
{
    public static class ThreadPool
    {
        public static int MaxThreadCount { get; set; }

        private static int _runningThreadCount;

        public static int _alivedThreadCount;

        private static byte _shutdown = 0;

        private static object _aliveCntLock = new();

        public static int TimeoutMilliseconds { get; set; } = 3000;

        private static readonly List<Thread> _coreThreadList = new();

        private static readonly Container.BlockingQueue<Action> _taskQueue = new();

        static ThreadPool()
        {
            _alivedThreadCount = 0;
            _runningThreadCount = 0;
            MaxThreadCount = Environment.ProcessorCount * 2;
            CoreStart();
        }

        public static void CoreStart()
        {
            var CoreThreadCount = Environment.ProcessorCount;
            for(int i = 0; i < CoreThreadCount; ++i)
            {
                var thread = new Thread(CoreThread);
                _coreThreadList.Add(thread);
                thread.Start();
                Interlocked.Increment(ref _alivedThreadCount);
            }
        }

        private static void CoreThread()
        {
            while(_shutdown == 0)
            {
                if(_taskQueue.Pop(out var task)) // pop会阻塞线程
                {
                    Interlocked.Increment(ref _runningThreadCount);
                    try
                    {
                        task?.Invoke();
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _runningThreadCount);
                    }
                }
            }
            Interlocked.Decrement(ref _alivedThreadCount);
        }

        public static bool Run(Action task)
        {
            if(_taskQueue.Count >= _runningThreadCount && _alivedThreadCount < MaxThreadCount)
            {
                var thread = new Thread(WorkerThread);
                thread.Start();
                Interlocked.Increment(ref _alivedThreadCount);
            }
            return _taskQueue.TryPush(task);
        }

        private static void WorkerThread()
        {
            try
            {
                while(_shutdown == 0)
                {
                    var isPopped = _taskQueue.Pop(out var task, TimeoutMilliseconds);
                    if(!isPopped)
                    {
                        break;
                    }
                    Interlocked.Increment(ref _runningThreadCount);
                    try
                    {
                        task?.Invoke();
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _runningThreadCount);
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref _alivedThreadCount);
            }
        }

        public static void WhenAll()
        {
            while(_runningThreadCount > 0 || !_taskQueue.IsEmpty())
            {
                Thread.Sleep(100);
            }
            Interlocked.Exchange(ref _shutdown, 1);
            _taskQueue.AddingComplete();
        }
    }
}