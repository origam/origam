using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam.Server
{
    internal static class AnalyticsFx
    {
        public static Analytics Instance {get;} = new Analytics(new PropertyProviderFactoryFx());
    }
}
