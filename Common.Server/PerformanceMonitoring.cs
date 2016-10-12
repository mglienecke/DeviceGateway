using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Instrumentation;
using System.Diagnostics;
using System.Globalization;

namespace Common.Server
{
    /// <summary>
    /// Service for writing performance monitoring data.
    /// </summary>
    public class PerformanceMonitoring : IDisposable
    {
        #region Constants...

        public const string PerfMonCentralServerServiceCategoryName = "CentralServerService";
        public const string PerfMonSensorDataStoring = "SensorDataStoring";

        internal const string PerfCounterTotalRequests = "TotalRequests";
        internal const string PerfCounterTotalRequestsPerSec = "TotalRequestsPerSecond";
        internal const string PerfCounterTotalRequestsCompleted = "TotalRequestsCompleted";
        internal const string PerfCounterRequestsCompletedPerSec = "RequestsCompletedPerSecond";
        //internal const string PerfCounterAverageRequestCompletionTime = "AverageRequestCompletionTime";
        internal const string PerfCounterLastRequestCompletionTime = "LastRequestCompletionTime";
        //internal const string PerfCounterRequestCompletionTimeTotal = "RequestCompletionTimeTotal";
        
        //internal const string PerfCounterNumberThreads = "NumRunningThreads";
        //internal const string PerfCounterTotalKBytesReturned = "TotalKBytesReturned";
        //internal const string PerfCounterAverageKBytesReturnedPerRequest = "AverageKBytesReturnedPerRequest";
        //internal const string PerfCounterAverageKBytesReturnedPerRequestBase = "AverageKBytesReturnedPerRequestBase";

        internal const string PerfCounterTotalRequestsDesc = "Total number of requests made";
        internal const string PerfCounterTotalRequestsPerSecDesc = "Number of requests made per second";
        internal const string PerfCounterTotalRequestsCompletedDesc = "Total number of requests completed";
        internal const string PerfCounterRequestsCompletedPerSecDesc = "Requests completed/Sec rate";
        //internal const string PerfCounterAverageRequestCompletionTimeDesc = "Average time required to complete a request";
        internal const string PerfCounterLastRequestCompletionTimeDesc = "Last time to complete a request";
        //internal const string PerfCounterRequestCompletionTimeTotalDesc = "Total time (in ticks) spent for all completed requests";

        //private const string PerfCounterNumberThreadsDesc = "Number of running threads";
        //internal const string PerfCounterTotalKBytesReturnedDesc = "Total number of KBytes returned as request results";
        //internal const string PerfCounterAverageKBytesReturnedPerRequestDesc = "Average number of KBytes returned per request";
        //internal const string PerfCounterAverageKBytesReturnedPerRequestBaseDesc = "Average number of KBytes returned per request (base counter)";
        #endregion

        const Int32 TimeSpanTicksPerSecond = 10000000;

        private readonly static Dictionary<string, PerformanceMonitoring> mMonitors = new Dictionary<string, PerformanceMonitoring>();
        private readonly Dictionary<string, PerformanceCounterGroup> mCounterGroups = new Dictionary<string, PerformanceCounterGroup>();
        private string mCategoryName;
        private static readonly Stopwatch mStopwatch = new Stopwatch();

        static PerformanceMonitoring()
        {
            mStopwatch.Start();
        }

        /// <summary>
        /// The method deletes a category with the passed name and then setups it anew.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="categoryDescription"></param>
        /// <param name="counterData"></param>
        /// <returns></returns>
        public static PerformanceCounterCategory SetupPerformanceMonitoringCategory(string categoryName, string categoryDescription, CounterCreationDataCollection counterData)
        {
            // Delete the category if it exists
            if (PerformanceCounterCategory.Exists(categoryName) == true)
                PerformanceCounterCategory.Delete(categoryName);

            //Create the category.
            return PerformanceCounterCategory.Create(categoryName, categoryDescription,
                PerformanceCounterCategoryType.MultiInstance, counterData);
        }

        #region Setup performance counters...
        /// <summary>
        /// Setup performance counter category.
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="categoryDescription"></param>
        public static void SetupPerformanceCounterCategoryForMethods(string categoryName, string categoryDescription)
        {
            //Checks
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }

            // Create the counter category if it does not exist
            if (PerformanceCounterCategory.Exists(categoryName) == true)
                PerformanceCounterCategory.Delete(categoryName);

            CounterCreationDataCollection counterData = new CounterCreationDataCollection();

            //Total requests made
            CounterCreationData totalRequestsMade = new CounterCreationData(PerfCounterTotalRequests,
                PerfCounterTotalRequestsDesc, PerformanceCounterType.NumberOfItems32);
            counterData.Add(totalRequestsMade);

            //Requests per second
            CounterCreationData requestsPerSecond = new CounterCreationData(PerfCounterTotalRequestsPerSec,
                PerfCounterTotalRequestsPerSecDesc, PerformanceCounterType.RateOfCountsPerSecond32);
            counterData.Add(requestsPerSecond);

            //Total requests completed
            CounterCreationData totalRequestsCompleted = new CounterCreationData(PerfCounterTotalRequestsCompleted,
                PerfCounterTotalRequestsCompletedDesc, PerformanceCounterType.NumberOfItems32);
            counterData.Add(totalRequestsCompleted);

            //Requests completed per second
            CounterCreationData requestsCompletedPerSecond = new CounterCreationData(PerfCounterRequestsCompletedPerSec,
                PerfCounterRequestsCompletedPerSecDesc, PerformanceCounterType.RateOfCountsPerSecond32);
            counterData.Add(requestsCompletedPerSecond);

            //Average request completion time
            //CounterCreationData averageRequestCompletionTime = new CounterCreationData(PerfCounterAverageRequestCompletionTime,
            //    PerfCounterAverageRequestCompletionTimeDesc, PerformanceCounterType.NumberOfItems64);
           // counterData.Add(averageRequestCompletionTime);
            //Total request completion time
            //CounterCreationData requestCompletionTimeTotal = new CounterCreationData(PerfCounterRequestCompletionTimeTotal,
            //    PerfCounterRequestCompletionTimeTotalDesc, PerformanceCounterType.NumberOfItems64);
            //counterData.Add(requestCompletionTimeTotal);
            //Last request completion time
            CounterCreationData lastRequestCompletionTime = new CounterCreationData(PerfCounterLastRequestCompletionTime,
                PerfCounterLastRequestCompletionTimeDesc, PerformanceCounterType.NumberOfItems64);
            counterData.Add(lastRequestCompletionTime);

            /*
            //Total KBytes returned 
            CounterCreationData totalKBytesReturned = new CounterCreationData(PerfCounterTotalKBytesReturned,
                PerfCounterTotalKBytesReturnedDesc, PerformanceCounterType.NumberOfItems64);
            counterData.Add(totalKBytesReturned);

            //Average KBytes returned per request
            CounterCreationData averageKBytesReturnedPerRequest = new CounterCreationData(PerfCounterAverageKBytesReturnedPerRequest,
                PerfCounterAverageKBytesReturnedPerRequestDesc, PerformanceCounterType.AverageCount64);
            CounterCreationData averageKBytesReturnedPerRequestBase = new CounterCreationData(PerfCounterAverageKBytesReturnedPerRequestBase,
                PerfCounterAverageKBytesReturnedPerRequestBaseDesc, PerformanceCounterType.AverageBase);
            counterData.Add(averageKBytesReturnedPerRequest);
            counterData.Add(averageKBytesReturnedPerRequestBase);
             * 
             *                 //Number of running threads
            //CounterCreationData numRunningThreads = new CounterCreationData(PerfCounterNumberThreads,
            //    PerfCounterNumberThreadsDesc, PerformanceCounterType.NumberOfItems32);
            //counterData.Add(numRunningThreads);
         */
            //Create the category.
            PerformanceCounterCategory.Create(categoryName, categoryDescription,
                PerformanceCounterCategoryType.MultiInstance, counterData);


            //Put into the hash if it is not there
            if (!mMonitors.ContainsKey(categoryName))
            {
                mMonitors[categoryName] = new PerformanceMonitoring(categoryName);
            }
        }

        /// <summary>
        /// The method creates a performance counter group with the specified instance name
        /// for the specified performance counter category. The category must be created before calling
        /// this method.
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="instanceName">The parameter value may not be null.</param>
        /// <seealso cref="SetupPerformanceCounterCategory"/>
        public static void SetupPerformanceCounters(string categoryName, string instanceName)
        {
            //Checks
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }
            if (instanceName == null)
            {
                throw new ArgumentNullException("instanceName");
            }

            //Setup the counters if the category is initialized.
            if (mMonitors.ContainsKey(categoryName))
            {
                mMonitors[categoryName].SetupPerformanceCounters(instanceName);
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    Properties.Resources.ExceptionPerformanceCounterCategoryNotSetUp, categoryName));
            }
        }
        #endregion

        #region Calling performance counters...
        /// <summary>
        /// Call this method when a new request has been made.
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="instanceName">The parameter value may not be null.</param>
        /// <returns>Time of the request.</returns>
        public static long IncrementRequestMade(string categoryName, string instanceName)
        {
            return mMonitors[categoryName].mCounterGroups[instanceName].IncRequestMade();
        }

        /// <summary>
        /// Call this method whenever a request is completed.
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="instanceName">The parameter value may not be null.</param>
        /// <param name="startTime">The request start time.</param>
        public static void IncrementRequestsCompleted(string categoryName, string instanceName, long startTimeTicks)
        {
            mMonitors[categoryName].mCounterGroups[instanceName].IncRequestsCompleted(startTimeTicks);
        }

        /*
        /// <summary>
        /// Call this method whenever a request returning results is completed.
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="instanceName">The parameter value may not be null.</param>
        /// <param name="bytes">Number of bytes returned.</param>
        public static void IncrementBytesReturned(string categoryName, string instanceName, int bytes) {
            mMonitors[categoryName].mCounterGroups[instanceName].IncrementBytesReturned(bytes);
        }
        */

        /*
        /// <summary>
        /// Call the method when a single item has been processed.
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="instanceName">The parameter value may not be null.</param>
        public static void IncrementItemsProcessed(string categoryName, string instanceName) {
            mMonitors[categoryName].mCounterGroups[instanceName].IncrementItemsProcessed();
        }

        /// <summary>
        /// Call the method when multiple items have been processed
        /// (instead of calling IncrementItemsProcessed multiple times)
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="instanceName">The parameter value may not be null.</param>
        /// <param name="incrementBy">Number of items processed.</param>
        public static void IncrementItemsProcessed(string categoryName, string instanceName, int incrementBy) {
            mMonitors[categoryName].mCounterGroups[instanceName].IncrementItemsProcessed(incrementBy);
        }
         */

        /*
        /// <summary>
        /// Call the method whenever a thread is created
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="instanceName">The parameter value may not be null.</param>
        public static void IncThreadCount(string categoryName, string instanceName) {
            mMonitors[categoryName].mCounterGroups[instanceName].IncThreadCount();
        }

        /// <summary>
        /// Call the method everytime a thread is killed/removed
        /// </summary>
        /// <param name="categoryName">The parameter value may not be null.</param>
        /// <param name="instanceName">The parameter value may not be null.</param>
        public static void DecThreadCount(string categoryName, string instanceName) {
            mMonitors[categoryName].mCounterGroups[instanceName].DecThreadCount();
        }
         */
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal PerformanceMonitoring(string categoryName)
        {
            mCategoryName = categoryName;
        }

        #region IDisposable members...
        /// <summary>
        /// Dispose the component.
        /// </summary>
        public void Dispose()
        {
            foreach (PerformanceCounterGroup group in mCounterGroups.Values)
            {
                group.RemovePerformanceCounters();
            }
        }
        #endregion

        /// <summary>
        /// The method creates a performance counter group with the specified instance name.
        /// </summary>
        /// <param name="instanceName">The parameter value may not be null.</param>
        /// <seealso cref="SetupPerformanceCounterCategory"/>
        private void SetupPerformanceCounters(string instanceName)
        {
            if (!mCounterGroups.ContainsKey(instanceName))
            {
                PerformanceCounterGroup group = new PerformanceCounterGroup(mCategoryName, instanceName);
                group.CreatePerformanceCounters();
                mCounterGroups[instanceName] = group;
            }
        }

        /// <summary>
        /// The class implements a group of performance counters configured to collect data
        /// for a specified instance name.
        /// </summary>
        internal class PerformanceCounterGroup : IDisposable
        {
            //Performance counters
            private PerformanceCounter mTotalRequestsMade;
            private PerformanceCounter mRequestsPerSecond;
            private PerformanceCounter mTotalRequestsCompleted;
            private PerformanceCounter mRequestsCompletedPerSecond;
            //private PerformanceCounter mAverageRequestCompletionTime;
            private PerformanceCounter mLastRequestCompletionTime;
            //private PerformanceCounter mRequestCompletionTimeTotal;
            //private PerformanceCounter mTotalKBytesReturned;
            //private PerformanceCounter mAverageKBytesReturnedPerRequest;
            //private PerformanceCounter mAverageKBytesReturnedPerRequestBase;
            //private PerformanceCounter mNumRunningThreads;

            private string mCategoryName;
            private string mInstanceName;

            #region IDisposable members...
            /// <summary>
            /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe
            /// oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
            /// </summary>
            public void Dispose()
            {
                if (mTotalRequestsMade != null)
                    mTotalRequestsMade.Dispose();
                if (mRequestsPerSecond != null)
                    mRequestsPerSecond.Dispose();
                if (mTotalRequestsCompleted != null)
                    mTotalRequestsCompleted.Dispose();
                if (mRequestsCompletedPerSecond != null)
                    mRequestsCompletedPerSecond.Dispose();
                //if (mAverageRequestCompletionTime != null)
                //    mAverageRequestCompletionTime.Dispose();
                if (mLastRequestCompletionTime != null)
                    mLastRequestCompletionTime.Dispose();
                //if (mRequestCompletionTimeTotal != null)
                //    mRequestCompletionTimeTotal.Dispose();
            }
            #endregion

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="categoryName">The parameter value may not be null.</param>
            /// <param name="instanceName">The parameter value may not be null.</param>
            internal PerformanceCounterGroup(string categoryName, string instanceName)
            {
                if (categoryName == null)
                {
                    throw new ArgumentNullException("categoryName");
                }
                if (instanceName == null)
                {
                    throw new ArgumentNullException("instanceName");
                }

                mCategoryName = categoryName;
                mInstanceName = instanceName;
            }

            /// <summary>
            /// The method creates and initializes the standard set of performance counters.
            /// </summary>
            internal void CreatePerformanceCounters()
            {
                //Create counters
                //Total requests made
                mTotalRequestsMade = new PerformanceCounter(mCategoryName,
                    PerformanceMonitoring.PerfCounterTotalRequests, mInstanceName, false);
                mTotalRequestsMade.RawValue = 0;

                //Requests per second
                mRequestsPerSecond = new PerformanceCounter(mCategoryName,
                    PerformanceMonitoring.PerfCounterTotalRequestsPerSec, mInstanceName, false);
                mRequestsPerSecond.RawValue = 0;

                //Total requests completed
                mTotalRequestsCompleted = new PerformanceCounter(mCategoryName,
                    PerformanceMonitoring.PerfCounterTotalRequestsCompleted, mInstanceName, false);
                mTotalRequestsCompleted.RawValue = 0;

                //Requests completed per second
                mRequestsCompletedPerSecond = new PerformanceCounter(mCategoryName,
                    PerformanceMonitoring.PerfCounterRequestsCompletedPerSec, mInstanceName, false);
                mRequestsCompletedPerSecond.RawValue = 0;

                //Average request completion time
                //mAverageRequestCompletionTime = new PerformanceCounter(mCategoryName,
                //    PerformanceMonitoring.PerfCounterAverageRequestCompletionTime, mInstanceName, false);
                //mAverageRequestCompletionTime.RawValue = 0;

                //Last request completion time
                mLastRequestCompletionTime = new PerformanceCounter(mCategoryName,
                    PerformanceMonitoring.PerfCounterLastRequestCompletionTime, mInstanceName, false);
                mLastRequestCompletionTime.RawValue = 0;

                //Total request completion time
                //mRequestCompletionTimeTotal = new PerformanceCounter(mCategoryName,
                //    PerformanceMonitoring.PerfCounterRequestCompletionTimeTotal, mInstanceName, false);
                //mRequestCompletionTimeTotal.RawValue = 0;

                //Total KBytes returned
                //mTotalKBytesReturned = new PerformanceCounter(mCategoryName,
                //    PerformanceMonitoring.PerfCounterTotalKBytesReturned, mInstanceName, false);
                //mTotalKBytesReturned.RawValue = 0;

                //Average KBytes returned
                //mAverageKBytesReturnedPerRequest = new PerformanceCounter(mCategoryName,
                //    PerformanceMonitoring.PerfCounterAverageKBytesReturnedPerRequest, mInstanceName, false);
                //mAverageKBytesReturnedPerRequest.RawValue = 0;

                //Average KBytes returned base 
                //mAverageKBytesReturnedPerRequestBase = new PerformanceCounter(mCategoryName,
                //    PerformanceMonitoring.PerfCounterAverageKBytesReturnedPerRequestBase, mInstanceName, false);
                //mAverageKBytesReturnedPerRequestBase.RawValue = 0;

                //mNumRunningThreads = new PerformanceCounter(mCategoryName,
                //    PerformanceMonitoring.PerfCounterNumThreads, m_serviceInstanceName, false);
                //mNumRunningThreads.RawValue = 0;
            }

            /// <summary>
            /// The method removes the initialized performance counter instances.
            /// </summary>
            internal void RemovePerformanceCounters()
            {
                mTotalRequestsMade.RemoveInstance();
                mRequestsPerSecond.RemoveInstance();
                mTotalRequestsCompleted.RemoveInstance();
                mRequestsCompletedPerSecond.RemoveInstance();
                //mAverageRequestCompletionTime.RemoveInstance();
                mLastRequestCompletionTime.RemoveInstance();
                //mRequestCompletionTimeTotal.RemoveInstance();
                //mTotalKBytesReturned.RemoveInstance();
                //mAverageKBytesReturnedPerRequest.RemoveInstance();
                //mAverageKBytesReturnedPerRequestBase.RemoveInstance();
                //mNumRunningThreads.RemoveInstance();
            }

            #region Calling perfomance counters...

            /// <summary>
            /// Call this method when a new request has been made.
            /// </summary>
            /// <returns>Internal time of the request in Stopwatch ticks.</returns>
            public long IncRequestMade()
            {
                mTotalRequestsMade.Increment();
                mRequestsPerSecond.Increment();
                return mStopwatch.ElapsedTicks;
            }

            /// <summary>
            /// Call this method whenever a request is completed.
            /// </summary>
            /// <param name="intervalStartTicks">The request start time.</param>
            public void IncRequestsCompleted(long intervalStartTicks)
            {
                //lock (this)
                //{
                    mTotalRequestsCompleted.Increment();
                    mRequestsCompletedPerSecond.Increment();

                    long requestDuration = mStopwatch.ElapsedTicks - intervalStartTicks;
                    mLastRequestCompletionTime.RawValue = requestDuration;

                    /*
                    if (mTotalRequestsCompleted.RawValue > 2000)
                    {
                        mRequestCompletionTimeTotal.IncrementBy(requestDuration);
                        mAverageRequestCompletionTime.RawValue = mRequestCompletionTimeTotal.RawValue / (mTotalRequestsCompleted.RawValue-2000);
                    }
                     */
                //}
            }

            /*
            /// <summary>
            /// Call the method when a requested data chunk if getting returned.
            /// </summary>
            /// <param name="bytes"></param>
            public void IncrementBytesReturned(long bytes) {
                long kBytes = bytes/1024;
                mTotalKBytesReturned.IncrementBy(kBytes);
                mAverageKBytesReturnedPerRequest.IncrementBy(kBytes);
                mAverageKBytesReturnedPerRequestBase.Increment();
            }
             */

            /*
            /// <summary>
            /// Call the method when a single item has been processed.
            /// </summary>
            public void IncrementItemsProcessed() {
                mItemsPerSecond.Increment();
            }

            /// <summary>
            /// Call the method when multiple items have been processed
            /// (instead of calling IncrementItemsProcessed multiple times)
            /// </summary>
            /// <param name="incrementBy">Number of items processed.</param>
            public void IncrementItemsProcessed(int incrementBy) {
                mItemsPerSecond.IncrementBy(incrementBy);
            }
             */

            /*
            /// <summary>
            /// Call the method whenever a thread is created
            /// </summary>
            public void IncThreadCount() {
                mNumRunningThreads.Increment();
            }

            /// <summary>
            /// Call the method everytime a thread is killed/removed
            /// </summary>
            public void DecThreadCount() {
                mNumRunningThreads.Decrement();
            }
             */

            #endregion
        }
    }
}

