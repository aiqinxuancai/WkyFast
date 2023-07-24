using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Utils
{
    internal class PathHelper
    {
        public static string RemoveInvalidChars(string input)
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            var invalidPathChars = Path.GetInvalidPathChars();

            foreach (var c in invalidFileNameChars.Union(invalidPathChars).Distinct())
            {
                input = input.Replace(c.ToString(), "");
            }

            return input;
        }
    }
}
