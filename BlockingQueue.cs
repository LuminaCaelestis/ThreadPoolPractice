using System.Collections.Generic;
using System.Threading;

namespace Flos.Container
{
    public class BlockingQueue<T>(int maxCapacity = -1)
    {
        private readonly Queue<T> _queue = new();

        private readonly object _lock = new();

        private readonly int _maxCapacity = maxCapacity;

        private bool _addingComplete = false;

        public bool IsFull() => _maxCapacity != -1 &&_queue.Count >= _maxCapacity;

        public bool IsEmpty() => _queue.Count == 0;

        public void AddingComplete()
        {
                _addingComplete = true;
                Monitor.PulseAll(_lock);
        }

        public bool TryPush(T item)
        {
            lock (_lock)
            {
                while (IsFull())
                {
                    if (_addingComplete) 
                    { 
                        return false; 
                    }
                    Monitor.Wait(_lock);
                }
                if (_addingComplete) 
                { 
                    return false; 
                }
                _queue.Enqueue(item);
                Monitor.PulseAll(_lock); // 仅在状态真正变化时通知消费者
                return true;
            }
        }

        public bool TryPop(out T item)
        {
            lock (_lock)
            {
                while (IsEmpty())
                {
                    if (_addingComplete)
                    {
                        #pragma warning disable CS8601 // 引用类型赋值可能为 null。
                        item = default;
                        return false;
                    }
                    Monitor.Wait(_lock);
                }
                item = _queue.Dequeue();
                Monitor.PulseAll(_lock); // 通知生产者队列空间可用
                return true;
            }
        }

        public bool TryPop(out T item, int millisecondsTimeout)
        {
            lock (_lock)
            {
                while (IsEmpty())
                {
                    if (_addingComplete)
                    {
                        #pragma warning disable CS8601 // 引用类型赋值可能为 null。
                        item = default;
                        return false;
                    }
                    Monitor.TryEnter
                }
                item = _queue.Dequeue();
                Monitor.PulseAll(_lock); // 通知生产者队列空间可用
                return true;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
                Monitor.PulseAll(_lock);
            }
        }

        public void AddingStart()
        {
            lock (_lock)
            {
                _addingComplete = false;
            }
        }       
    }
}