using System;
using Microsoft.SPOT;
using System.Threading;

namespace DeviceServer.Base
{
    /// <summary>
    /// The class implements a simple one-element buffer with notifications for exchanging data between threads.
    /// </summary>
    public sealed class WaitingBuffer
    {
        private readonly Object mLockObject = new object();
        private readonly ManualResetEvent mSignal = new ManualResetEvent(false);
        private object buffer = null;

        /// <summary>
        /// Puts object o, which may be null, into the buffer.
        /// The current buffer contents is overwritten with the new one.
        /// It is safe to call this method from different threads.
        /// </summary>
        /// <param name="o">New value to be buffered.</param>
        public void HandlePut(object o)
        {
            lock (mLockObject)
            {
                //Set the buffer, signal the change
                buffer = o;
                mSignal.Set();
            }
        }

        /// <summary>
        /// Returns the object that is currently contained in the buffer.
        /// The method blocks if the currently contained object is null until the object is set by the <see cref="HandlePut"/> method.
        /// It is safe to call this method from different threads.
        /// </summary>
        /// <returns>Currently buffered value. May be null.</returns>
        public object HandleGet()
        {
            object result;

            //Wait until signalled that the buffer is set
            mSignal.WaitOne();

            lock (mLockObject)
            {
                //Read the result, clean the buffer
                result = buffer;
                buffer = null;
                mSignal.Reset();
            }
            return result;
        }
    }
}
