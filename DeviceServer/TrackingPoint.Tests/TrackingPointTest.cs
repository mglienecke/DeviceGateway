using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace TrackingPoint.Tests
{
    /// <summary>
    /// The class implements unit tests for the <see cref="TrackingPoint"/> class methods.
    /// </summary>
    [TestClass]
    public class TrackingPointTest
    {
        #region Zusätzliche Testattribute
        //
        // Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
        //
        // Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var connectionStr = ConfigurationManager.ConnectionStrings["ConnectionString"];
            if (connectionStr == null)
                throw new ArgumentNullException("Missing connection string \"ConnectionString\" in the app.config");

            ConnectionString = connectionStr.ConnectionString;

        }

        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        // [ClassCleanup()]
        public static void MyClassCleanup() { }

        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private static string ConnectionString { get; set; }

        [TestMethod]
        public void CreateTrackingPointWithTimestamp()
        {
            TrackingPoint.CreateTrackingPoint(ConnectionString, "TestPoint", "TestAdditionalData", DateTime.Now.AddDays(-1), 0, null);
        }

        [TestMethod]
        public void CreateTrackingPointWithTimestamp_NullAdditionalData()
        {
            TrackingPoint.CreateTrackingPoint(ConnectionString, "TestPoint", null, DateTime.Now.AddDays(-1), 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTrackingPointWithTimestamp_NullTrackingPoint()
        {
            TrackingPoint.CreateTrackingPoint(ConnectionString, null, "TestAdditionalData", DateTime.Now, 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTrackingPointWithTimestamp_NullConnectionString()
        {
            TrackingPoint.CreateTrackingPoint(null, "TestPoint", "TestAdditionalData", DateTime.Now, 0, null);
        }

        [TestMethod]
        public void CreateTrackingPoint()
        {
            TrackingPoint.CreateTrackingPoint(ConnectionString, "TestPoint", "TestAdditionalData", 0, null);
        }

        [TestMethod]
        public void CreateTrackingPoint_NullAdditionalData()
        {
            TrackingPoint.CreateTrackingPoint(ConnectionString, "TestPoint", null, 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTrackingPoint_NullTrackingPoint()
        {
            TrackingPoint.CreateTrackingPoint(ConnectionString, null, "TestAdditionalData", 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTrackingPoint_NullConnectionString()
        {
            TrackingPoint.CreateTrackingPoint(null, "TestPoint", "TestAdditionalData", 0, null);
        }

        [TestMethod]
        public void CreateTrackingPointWithTimestamp_DefaultConnectionString()
        {
            TrackingPoint.CreateTrackingPoint("TestPoint", "TestAdditionalData", DateTime.Now.AddDays(-1));
        }

        [TestMethod]
        public void CreateTrackingPointWithTimestamp_DefaultConnectionString_NullAdditionalData()
        {
            TrackingPoint.CreateTrackingPoint("TestPoint", null, DateTime.Now.AddDays(-1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTrackingPointWithTimestamp_DefaultConnectionString_NullTrackingPoint()
        {
            TrackingPoint.CreateTrackingPoint(null, "TestAdditionalData", DateTime.Now);
        }

        [TestMethod]
        public void CreateTrackingPoint_DefaultConnectionString()
        {
            TrackingPoint.CreateTrackingPoint("TestPoint", "TestAdditionalData");
        }

        [TestMethod]
        public void CreateTrackingPoint_DefaultConnectionString_NullAdditionalData()
        {
            TrackingPoint.CreateTrackingPoint("TestPoint", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTrackingPoint_DefaultConnectionString_NullTrackingPoint()
        {
            TrackingPoint.CreateTrackingPoint(null, "TestAdditionalData");
        }
    }
}
