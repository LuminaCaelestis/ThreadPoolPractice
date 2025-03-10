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

        private static bool _shutdown = false;

        public static int TimeoutMilliseconds { get; set; } = 3000;

        private static readonly List<Thread> _coreThreadList = new();

        private static readonly Container.BlockingQueue<Action> _taskQueue = new();

        static ThreadPool()
        {
            _alivedThreadCount = 0;
            _runningThreadCount = 0;
            MaxThreadCount = Environment.ProcessorCount * 2;
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
            while(!_shutdown)
            {
                if(_taskQueue.Pop(out var task)) // pop会阻塞线程
                {
                    Interlocked.Increment(ref _runningThreadCount);
                    task?.Invoke();
                    Interlocked.Decrement(ref _runningThreadCount);
                }
            }
        }

        public static void Run(Action task)
        {
            if(_runningThreadCount >= _alivedThreadCount && _alivedThreadCount < MaxThreadCount)
            {
                var thread = new Thread(WorkerThread);
                thread.Start();
                Interlocked.Increment(ref _alivedThreadCount);
            }
            _taskQueue.TryPush(task);
        }

        private static void WorkerThread()
        {
            while(!_shutdown)
            {
                var isPopped = _taskQueue.Pop(out var task, TimeoutMilliseconds);
                if(!isPopped)
                {
                    break;
                }
                Interlocked.Increment(ref _runningThreadCount);
                task?.Invoke();
                Interlocked.Decrement(ref _runningThreadCount);
            }
        }

        public static void WhenAll()
        {
            while(_runningThreadCount > 0 || !_taskQueue.IsEmpty())
            {
                Thread.Sleep(100);
            }
            _taskQueue.AddingComplete();
            _shutdown = true;
        }
    }
}