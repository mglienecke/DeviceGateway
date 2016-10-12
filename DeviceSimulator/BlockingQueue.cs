using System;
using System.Collections;
using System.Threading;

using System.Runtime.CompilerServices;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// Same as Queue except Dequeue function blocks until there is an object to return.
    /// Note: This class does not need to be synchronized
    /// </summary>
    public class BlockingQueue : LimitedQueue
    {
        private readonly ManualResetEvent mHasValue = new ManualResetEvent(false);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BlockingQueue()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public BlockingQueue(int maxCapacity):base(maxCapacity)
        {
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~BlockingQueue()
        {
            Clear();
        }

        /// <summary>
        /// The method removes all objects from the Queue.
        /// </summary>
        public override void Clear()
        {
            lock (base.SyncRoot)
            {
                base.Clear();
                mHasValue.Set();
            }
        }

        /// <summary>
        /// The method removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <returns>Object in the queue.</returns>
        public override object Dequeue()
        {
            return Dequeue(Timeout.Infinite);
        }

        /// <summary>
        /// The method removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <param name="timeout">time to wait before failing with an <see cref="InvalidOperationException"/></param>
        /// <returns>Object in the queue.</returns>
        public object Dequeue(TimeSpan timeout)
        {
            return Dequeue(timeout.Milliseconds);
        }

        /// <summary>
        /// The method removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <param name="timeout">time to wait before failing with an <see cref="InvalidOperationException"/> (in milliseconds)</param>
        /// <returns>Object in the queue.</returns>
        public object Dequeue(int timeout)
        {
            //Wait if the queue is empty
            if (Count == 0)
            {
                mHasValue.WaitOne(Int32.MaxValue, true);
            }

            lock (base.SyncRoot)
            {
                if (Count > 0)
                {
                    object result = base.Dequeue();
                    //Stop the next threads if the queue is empty
                    if (Count == 0)
                    {
                        mHasValue.Reset();
                    }
                    return result;
                }
                else
                {
                    return null;
                    //throw new InvalidOperationException("Queue has been cleared");
                }
            }
        }

        /// <summary>
        /// The method adds an object to the end of the queue.
        /// </summary>
        /// <param name="obj"></param>
        public override void Enqueue(object obj)
        {
            lock (base.SyncRoot)
            {
                base.Enqueue(obj);
                mHasValue.Set();
            }
        }
    }
}

