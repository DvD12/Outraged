using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Outraged
{
    public class DataHeaders
    {
        private static string PersistentDataPath = Application.persistentDataPath;

        public static JsonSerializerSettings GetFormatting(Formatting formatting = Formatting.None, DefaultValueHandling defaultValue = DefaultValueHandling.Ignore)
        {
            JsonSerializerSettings s = new JsonSerializerSettings();
            s.Converters.Add(new StringEnumConverter());
            s.NullValueHandling = NullValueHandling.Ignore;
            s.ObjectCreationHandling = ObjectCreationHandling.Replace; // without this, you end up with duplicates.
            s.DefaultValueHandling = defaultValue;
            s.Formatting = formatting;
            return s;
        }
        public static string GetPersistentDataPath()
        {
            if (string.IsNullOrEmpty(PersistentDataPath))
                PersistentDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            if (/*!Application.isMobilePlatform && */!Directory.Exists(PersistentDataPath))
            {
                Directory.CreateDirectory(PersistentDataPath);
            }
            return PersistentDataPath;
        }
    }
}
