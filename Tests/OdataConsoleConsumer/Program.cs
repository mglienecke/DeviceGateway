using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using OdataConsoleConsumer.OdataConsoleConsumer;

namespace OdataConsoleConsumer
{
    class Program
    {
        static PerformanceCounter cpuCounter;
        static PerformanceCounter ramCounter;

        public static int MeasuredCpuInPercent { get; set; }
        public static int MeasuredRamInMB { get; set; }
        public static bool stopTasks;

        static void Main(string[] args)
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            Task cpuTask = Task.Run
                (
                 () =>
                 {
                     while (true)
                     {
                         MeasuredCpuInPercent = (MeasuredCpuInPercent + Convert.ToInt32(cpuCounter.NextValue())) / 2;
                         Thread.Sleep(100);
                         if (stopTasks) return;
                     }
                 });

            Task ramTask = Task.Run
                (
                 () =>
                 {
                     while (true)
                     {
                         MeasuredRamInMB = (MeasuredRamInMB + Convert.ToInt32(GC.GetTotalMemory(false) / (1024 * 1024))) / 2;
                         Thread.Sleep(100);
                         if (stopTasks) return;
                     }
                 });

            try
            {

                var entities = new OdataConsoleConsumer.Entities(new Uri(ConfigurationManager.AppSettings["Url"]));
                var query = from sd in entities.SensorData select sd;

                // reset the values after an initial read to get rid of swing in phase
                List<SensorData> sdList = query.Take(1000).ToList();
                MeasuredCpuInPercent = 0;
                MeasuredRamInMB = 0;

                // wait a little as well
                Thread.Sleep(300);
                Console.WriteLine("CPU-Load Start = {0} %, RAM = {1} MB", MeasuredCpuInPercent, MeasuredRamInMB);

                // now the real iterations start with reading the defined amount (always increasing) and then dumping some results
                Stopwatch sw = new Stopwatch();
                for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetrievedRecords"]); i += Convert.ToInt32(ConfigurationManager.AppSettings["RecordIncrement"]))
                {
                    sw.Restart();
                    sdList = query.Take(i).ToList();
                    sw.Stop();


                    // write 3 values (msec, % CPU and RAM) for each iteration (which is the correlation id as well)
                    TrackingPoint.TrackingPoint.CreateTrackingPoint("ODATA client", "request duration", sw.ElapsedMilliseconds, i.ToString());
                    TrackingPoint.TrackingPoint.CreateTrackingPoint("ODATA client", "CPU", MeasuredCpuInPercent, i.ToString());
                    TrackingPoint.TrackingPoint.CreateTrackingPoint("ODATA client", "RAM", MeasuredRamInMB, i.ToString());

                    Console.WriteLine("Finished reading {0} data-items in {1} msec, CPU-load was {2} %, RAM = {3} MB", sdList.Count, sw.ElapsedMilliseconds, MeasuredCpuInPercent, MeasuredRamInMB);
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("Exception while reading data: {0}", x);
            }

            stopTasks = true;
            cpuTask.Wait(200);
            ramTask.Wait(200);

            Console.WriteLine("Press ENTER");
            Console.ReadLine();
        }



    }
}

