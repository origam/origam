using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.DA.Service.FileSystemModeCheckers
{
    public class ModelErrorSection
    {
        public ModelErrorSection(string caption, List<string> errorMessages)
        {
            Caption = caption;
            ErrorMessages = errorMessages;
        }

        public string Caption { get; }
        public List<string> ErrorMessages { get; }

        public bool IsEmpty => ErrorMessages == null || ErrorMessages.Count == 0;

        public override string ToString()
        {
            return Caption + ":\n" + string.Join("\n", ErrorMessages);
        }
    }
}
