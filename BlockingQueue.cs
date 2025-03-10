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
                if(IsFull() || _addingComplete)
                {
                    return false;
                }
                _queue.Enqueue(item);
                Monitor.Pulse(_lock);
                return true;
            }
        }

        public bool Pop(out T item)
        {
            lock (_lock)
            {
                while (IsEmpty())
                {
                    if (_addingComplete)
                    {
                        #pragma warning disable CS8601
                        item = default;
                        return false;
                    }
                    Monitor.Wait(_lock);
                }
                item = _queue.Dequeue();
                Monitor.Pulse(_lock);
                return true;
            }
        }

        public bool Pop(out T? item, int millisecondsTimeout)
        {
            lock (_lock)
            {
                while (IsEmpty())
                {
                    if (_addingComplete || Monitor.Wait(_lock, millisecondsTimeout))
                    {
                        #pragma warning disable CS8601
                        item = default;
                        return false;
                    }
                }
                item = _queue.Dequeue();
                Monitor.Pulse(_lock);
                return true;
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