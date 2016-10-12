using System;


namespace DeviceServer.Simulator
{
    [Serializable]
    public class Location
    {

        public string Name { get; set; }


        public double Latitude { get; set; }


        public double Longitude { get; set; }


        public double Elevation { get; set; }
    }

    [Serializable]
    public class Device
    {

        public string Id { get; set; }


        public string Description { get; set; }


        public Location Location { get; set; }


        public string DeviceIpEndPoint { get; set; }
    }
}
