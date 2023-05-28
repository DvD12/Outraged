using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Outraged
{

    // Find that finds all disabled objects
    public partial class Helpers : System.Object
    {
        private static Dictionary<System.Type, Dictionary<string, Object>> cache = new Dictionary<System.Type, Dictionary<string, Object>>();

        public static T Find<T>(string name) where T : class
        {
            return Find(name, typeof(T)) as T;
        }
        public static Object Find(string name, System.Type type)
        {
            Object obj = null;
            if (cache.ContainsKey(type) && cache[type].ContainsKey(name))
            {
                obj = cache[type][name];
            }
            //Not found
            if (obj == null || obj.name.Contains("_Deleted"))
            {
                var objects = Resources.FindObjectsOfTypeAll(type).Select(x => (Object)x).ToList();
                if (!cache.ContainsKey(type))
                {
                    cache.Add(type, new Dictionary<string, Object>());
                }
                foreach (var o in objects)
                {
                    if (!cache[type].ContainsKey(o.name))
                    {
                        cache[type].Add(o.name, o);
                    }
                    else
                    {
                        cache[type][o.name] = o;
                    }
                    if (o.name == name)
                    {
                        obj = o;
                    }
                }
            }
            return obj;
        }
    }
}