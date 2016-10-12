using System;

using System.Collections;

namespace DeviceServer.Simulator
{
    [Serializable]
    public class LimitedQueue:Queue
    {
        private int mMaxCount = Int32.MaxValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LimitedQueue()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxCount"></param>
        public LimitedQueue(int maxCount)
        {
            MaxCount = maxCount;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="collection"></param>
        public LimitedQueue(ICollection collection)
            :base(collection)
        {
        }

        public int MaxCount
        {
            get
            {
                return mMaxCount;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                else
                {
                    mMaxCount = value;
                    TrimQueue();
                }
            }
        }

        public override void Enqueue(object obj)
        {
            base.Enqueue(obj);

            //Trim the end if the max count is exceeded.
            if (Count > MaxCount)
            {
                this.Dequeue();
            }
        }

        private void TrimQueue()
        {
            if (MaxCount > 0)
            {
                //Make the queue's count no larger than the MaxCount
                while (Count > MaxCount)
                {
                    this.Dequeue();
                }
            }
        }
    }
}
