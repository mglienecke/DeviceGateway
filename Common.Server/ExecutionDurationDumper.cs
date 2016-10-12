using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Server
{
    /// <summary>
    /// Helper class for managing execution duration dumps. It stores several sections with lists of execution timings as well as provides extension methods for the Stopwatch class to handle µsec easier. 
    /// </summary>
    public static class ExecutionDurationDumper
    {
        private static readonly Dictionary<string, List<long>> DurationDict = new Dictionary<string, List<long>>();

        /// <summary>
        /// Nanosekunden / Tick der Uhr
        /// </summary>
        public static readonly long NanoSecPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;

        /// <summary>
        /// Get the list of execution durations
        /// </summary>
        /// <param name="section">the section which is relevant</param>
        /// <returns>a list of measures</returns>
        public static List<long> GetExecutionDurationsForSection(string section)
        {
            lock (DurationDict)
            {
                if (DurationDict.ContainsKey(section))
                {
                    return (DurationDict[section]);
                }
            }
            return (null);
        }

        /// <summary>
        /// Clear anything in the dictionary
        /// </summary>
        public static void Clear()
        {
            lock (DurationDict)
            {
                DurationDict.Clear();
            }
        }

        /// <summary>
        /// Get a list of all sections which are defined
        /// </summary>
        public static List<string> Sections
        {
            get { return (DurationDict.Keys.ToList()); }
        }

        /// <summary>
        /// Adds a execution duration in µsec to the specified section list. If the section is not present, a new section is created.
        /// 
        /// This is designed as an extension method for Stopwatch
        /// </summary>
        /// <param name="watch">the stopwatch to use</param>
        /// <param name="section">which section to store the value</param>
        public static void AddExecutionDurationToList(this Stopwatch watch, string section)
        {
            lock (DurationDict)
            {
                if (DurationDict.ContainsKey(section) == false)
                {
                    DurationDict.Add(section, new List<long>());
                }

                DurationDict[section].Add((watch.ElapsedTicks * NanoSecPerTick) / 1000);
            }
        }
       
        /// <summary>
        /// Return the elapsed ticks of the watch in µsec
        /// </summary>
        /// <param name="watch">the watch to use</param>
        /// <returns>the time in µsec</returns>
        public static long ElapsedMicroSeconds(this Stopwatch watch)
        {
            return ((watch.ElapsedTicks*NanoSecPerTick)/1000);
        }
    }
}
