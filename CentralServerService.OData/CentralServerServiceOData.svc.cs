
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CentralServerService.OData
{
    public class CentralServerServiceOData : EntityFrameworkDataService<Entities>
    {
        public static Stopwatch requestWatch;

        // Diese Methode wird nur einmal aufgerufen, um dienstweite Richtlinien zu initialisieren.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // TODO: Regeln festlegen, die angeben, welche Entitätssets und welche Dienstvorgänge sichtbar, aktualisierbar usw. sind
            // Beispiele:
            config.SetEntitySetAccessRule("Devices", EntitySetRights.AllRead);
            config.SetEntitySetAccessRule("Sensors", EntitySetRights.AllRead);
            config.SetEntitySetAccessRule("SensorData", EntitySetRights.All);
            // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.DataServiceBehavior.AcceptCountRequests = true;
            config.DataServiceBehavior.AcceptProjectionRequests = true;

            requestWatch = new Stopwatch();
        }

        protected override void OnStartProcessingRequest(ProcessRequestArgs args)
        {
            
            requestWatch.Restart();
            base.OnStartProcessingRequest(args);
            ProcessingPipeline.ProcessedRequest += (sender, eventArgs) =>
            {
                requestWatch.Stop();
                TrackingPoint.TrackingPoint.CreateTrackingPoint("ODATA service", "proc. req. in msec", requestWatch.ElapsedMilliseconds, eventArgs.OperationContext.AbsoluteRequestUri.ToString());
            };
        }

        //// Define a query interceptor for the Orders entity set.
        //[QueryInterceptor("SensorData")]
        //public Expression<Func<SensorData, bool>> OnQuerySensorData()
        //{
        //    TrackingPoint.TrackingPoint.CreateTrackingPoint("ODATA", "on query sensor data");

        //    return sd => true;
        //}
    }

}
