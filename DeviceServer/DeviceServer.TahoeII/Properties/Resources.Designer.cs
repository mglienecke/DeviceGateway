//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.18444
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DeviceServer.vTahoeII.Properties
{
    
    internal partial class Resources
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((Resources.manager == null))
                {
                    Resources.manager = new System.Resources.ResourceManager("DeviceServer.vTahoeII.Properties.Resources", typeof(Resources).Assembly);
                }
                return Resources.manager;
            }
        }
        internal static string GetString(Resources.StringResources id)
        {
            return ((string)(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        [System.SerializableAttribute()]
        internal enum StringResources : short
        {
            SensorUnitSymbolBat = -29943,
            SensorUnitSymbolButtonSW7 = -27615,
            DescriptionSensorAux = -20918,
            DefaultSensorCategoryName = -17046,
            DeviceLocationNameDefault = -421,
            DescriptionSensorTemperature = -6,
            DescriptionBoard = 368,
            SensorUnitSymbolAux = 21067,
            DescriptionSensorBat = 23395,
            DescriptionSensorButtonSW7 = 23600,
            SensorUnitSymbolTemperature = 32050,
        }
    }
}