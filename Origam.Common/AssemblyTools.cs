using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Origam
{
    class AssemblyTools
    {
        public static string GetAssemblyLocation()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
            //return Path.GetDirectoryName(path);
        }
    }
}
