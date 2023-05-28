using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
[assembly: AssemblyVersion("1.0.*")]
namespace Outraged
{
    // http://luminaryapps.com/blog/showing-the-build-date-in-a-unity-app/
    public class BuildDate
    {
        private static string format = "yyyyMMdd-HHmmss";

        public static System.Version Version()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static System.DateTime Date()
        {
            System.Version version = Version();
            System.DateTime startDate = new System.DateTime(2000, 1, 1, 0, 0, 0);
            System.TimeSpan span = new System.TimeSpan(version.Build, 0, 0, version.Revision * 2);
            System.DateTime buildDate = startDate.Add(span);
            return buildDate;
        }

        public static string ToString()
        {
            try
            {
                System.DateTime date = Date();
                return date.ToString(format);
            }
            catch { }
            return "unknown";
        }
    }
}
