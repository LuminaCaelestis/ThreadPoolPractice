using System;
using System.Collections.Generic;
using System.Threading;

namespace Flos.Threading
{
    public class ThreadPool
    {
        public int MaxThreadCount { get; set; }

        private int _runningThreadCount;

        public int RunningThreadCount =>  _runningThreadCount;

        private bool _shutdown = false;

        private readonly List<Thread> _threadList = new();

        private readonly Container.BlockingQueue<Action> _taskQueue = new();

        // 禁用拷贝构造
        private ThreadPool(ThreadPool other)
        {
            throw new NotSupportedException("Copy constructor is not supported");
        }

        // 默认构造
        public ThreadPool() : this(Environment.ProcessorCount)
        {
            _runningThreadCount = 0;
            MaxThreadCount = Environment.ProcessorCount;
        }

        public ThreadPool(int numCoreThread)
        {
            if(numCoreThread < 1)
            {
                throw new ArgumentException("AvalibleThreadCount must be greater than 0");
            }
            _idleThreadCount = numCoreThread;
            MaxThreadCount = numCoreThread;
        }

        public void Shutdown()
        {
            _shutdown = true;
            _workQueue.PulseAll();
            foreach (Thread thread in _threads)
            {
                thread.Join();
            }
            _threads.Clear();
        }

        public bool AddTask(Action task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            if (_shutdown)
            {
                throw new InvalidOperationException("ThreadPool is shutting down");
            }
            return _workQueue.TryEnqueue(task);
        }

        private void Worker()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started");
            // 工作线程
            while (!_shutdown)
            {
                try
                {
                    Interlocked.Decrement(ref _idleThreadCount);
                    Action method = _workQueue.Dequeue();
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Invoke {method.Method.Name}");
                    method.Invoke();
                }
                catch (Exception e)
                {
                    throw new Exception("An error occurred in a thread", e);
                }
                finally
                {
                    Interlocked.Increment(ref _idleThreadCount);
                }
            }
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} stopped");
        }

        public void Start()
        {
            Interlocked.Exchange(ref _shutdown, false);
            for (int i = 0; i < _idleThreadCount; ++i)
            {
                Thread thread = new Thread(Worker);
                _threads.Add(thread);
                thread.Start();
            }
        }
    }
}