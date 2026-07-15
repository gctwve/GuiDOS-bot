using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YourCheese
{
    class Constants
    {
        public static String FILE_LOCATION = ResolveFileLocation();

        private static string ResolveFileLocation()
        {
            var candidates = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameAgent"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "GameAgent"),
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "GameAgent"),
                Path.Combine(Directory.GetCurrentDirectory(), "GameAgent")
            };

            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(Path.Combine(fullPath, "Skeld", "regions.json")))
                {
                    return fullPath;
                }
            }

            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameAgent"));
        }
    }
}
