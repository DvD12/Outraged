using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outraged
{
    public class TaskController
    {
        public static Dictionary<string, Outraged.Task> Tasks = new Dictionary<string, Outraged.Task>();

        public static Outraged.Task Create(string key, System.Collections.IEnumerator c)
        {
            Outraged.Task t;
            if (!Tasks.ContainsKey(key) || !Tasks[key].Running)
            {
                t = new Outraged.Task(c);
                Tasks[key] = t;
            }
            if (!Tasks[key].Running) { Tasks[key].Start(); }
            return Tasks[key];
        }
        public static void Stop(string key)
        {
            if (Tasks.ContainsKey(key) && Tasks[key].Running)
            {
                Tasks[key].Stop();
            }
        }
    }
}
