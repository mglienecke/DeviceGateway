using System;
using Microsoft.SPOT;
using System.Collections;
using System.IO;
using System.Xml;

namespace DeviceServer.Base
{
    public static class ConfigurationManager
    {
        private const string XmlNodeAppSection = "appSettings";
        private const string XmlNodeAdd        = "add";
        private const string XmlAttributeKey   = "key";
        private const string XmlAttributeValue = "value";

        private static Hashtable mAppSettings;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ConfigurationManager()
        {
            mAppSettings = new Hashtable();
        }

        /// <summary>
        /// The method returns value stored in the config data with the passed key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Returns <c>null</c> if no value stored with the passed key.</returns>
        public static string GetAppSetting(string key)
        {
            return GetAppSetting(key, null);
        }

        /// <summary>
        /// The method returns value stored in the config data with the passed key with a default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetAppSetting(string key, string defaultValue)
        {
            if (!mAppSettings.Contains(key))
                return defaultValue;
            return (string)mAppSettings[key];
        }

        /// <summary>
        /// The method loads appSettings-style configuration data from the provided stream.
        /// </summary>
        /// <param name="xmlStream"></param>
        public static void Load(Stream xmlStream)
        {
            using (XmlReader reader = XmlReader.Create(xmlStream))
            {
                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case XmlNodeAppSection:
                            while (reader.Read())
                            {
                                if (reader.Name == XmlNodeAppSection)
                                    break;

                                if (reader.Name == XmlNodeAdd)
                                {
                                    var key = reader.GetAttribute(XmlAttributeKey);
                                    var value = reader.GetAttribute(XmlAttributeValue);

                                    mAppSettings.Add(key, value);
                                }
                            }

                            break;
                    }
                }
            }
        }
    }

}
