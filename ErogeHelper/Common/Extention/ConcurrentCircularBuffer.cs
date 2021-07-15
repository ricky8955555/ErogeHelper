using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ErogeHelper.Common.Extention
{
    // Sources: 
    // https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/
    // https://docs.microsoft.com/en-us/dotnet/api/system.threading.interlocked?view=netcore-3.1
    // https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Collections/Concurrent/ConcurrentQueue.cs
    // https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Collections/Concurrent/ConcurrentQueueSegment.cs

    /// <summary>
    /// Concurrent safe circular buffer that will used a fixed capacity specified and resuse slots as it goes.
    /// </summary>
    /// <typeparam name="TObject">The object that you want to go into the slots.</typeparam>
    public class ConcurrentCircularBuffer<TObject> : IReadOnlyCollection<TObject>
    {
        private readonly ConcurrentQueue<TObject> _queue;

        public int Capacity { get; }

        public int Count => _queue.Count;

        public ConcurrentCircularBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException($@"The capacity specified '{capacity}' is not valid.", nameof(capacity));
            }

            // Setup the queue to the initial capacity using List's underlying implementation.
            _queue = new ConcurrentQueue<TObject>(new List<TObject>(capacity));

            Capacity = capacity;
        }

        public void Enqueue(TObject @object)
        {
            // Enforce the capacity first so the head can be used instead of the entire segment (slow).
            while (_queue.Count + 1 > Capacity)
            {
                if (_queue.TryDequeue(out _))
                    continue;

                // Handle error condition however you want to ie throw, return validation object, etc.
                var ex = new Exception("Concurrent Dequeue operation failed.");
                ex.Data.Add("EnqueueObject", @object);
                throw ex;
            }

            // Place the item into the queue
            _queue.Enqueue(@object);
        }

        public TObject? Dequeue()
        {
            if (_queue.TryDequeue(out var result))
            {
                return result;
            }

            return default;
        }

        public IEnumerator<TObject> GetEnumerator()
        {
            return new List<TObject>(_queue).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}