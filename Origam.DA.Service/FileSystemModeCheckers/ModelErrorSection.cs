using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.DA.Service.FileSystemModeCheckers
{
    public class ModelErrorSection
    {
        public string Caption { get; set; }
        public List<string> ErrorMessages { get; set; }

        public override string ToString()
        {
            return Caption + ":\n" + string.Join("\n", ErrorMessages);
        }
    }
}
