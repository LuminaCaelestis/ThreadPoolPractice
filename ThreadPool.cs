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
                var isPopped = _taskQueue.TryPop(out var task);
                if(!isPopped)
                {
                    continue;
                }
                task?.Invoke();
            }
        }

        public static void WorkerThread()
        {
        }

    }
}