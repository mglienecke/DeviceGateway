using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Common.Server
{
    /// <summary>
    /// The class implements a blocking queue whose size can be limited.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlockingQueue<T>
    {
        private readonly Queue<T> mQueue = new Queue<T>();
        private readonly int mMaxSize;
        private bool mDiscardNewIfSizeExceeded;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="maxSize"></param>
        public BlockingQueue() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxSize"></param>
        public BlockingQueue(int maxSize) { this.mMaxSize = maxSize; }

        /// <summary>
        /// The maximum size of the queue (0 - unlimited)
        /// </summary>
        public int MaxSize { get { return mMaxSize; } }

        /// <summary>
        /// Flag defining if new enqueued values are to be discarded if the queue size gets exceeded 
        /// (<c>true</c> means that the last enqueued vitems will be discarded, the queue doesn't block
        /// <c>false</c> means that the queue will block the enqueueing threads until an item is dequeued).
        /// </summary>
        public bool DiscardNewIfSizeExceeded
        {
            get { return mDiscardNewIfSizeExceeded; }
            set { mDiscardNewIfSizeExceeded = value; }
        }

        /// <summary>
        /// Enqueue a new item into the queue.
        /// </summary>
        /// <param name="item"></param>
        /// <returns><c>true</c> if the item has been successfully dequeued</returns>
        public bool Enqueue(T item)
        {
            lock (mQueue)
            {
                //Check if the size limitation checks in
                while (mMaxSize != 0 && mQueue.Count >= mMaxSize)
                {
                    if (mDiscardNewIfSizeExceeded)
                        return false;
                    else
                        Monitor.Wait(mQueue);
                }

                //Enqueue
                mQueue.Enqueue(item);

                //Notify
                Monitor.Pulse(mQueue);

                return true;
            }
        }

        /// <summary>
        /// Dequeue an item from the queue.
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            lock (mQueue)
            {
                //Check if any elements inside
                while (mQueue.Count == 0)
                {
                    Monitor.Wait(mQueue);
                }

                //Dequeue
                T item = mQueue.Dequeue();

                //Notify if not empty
                if (mQueue.Count > 0) Monitor.PulseAll(mQueue);

                return item;
            }
        }
    }
}
