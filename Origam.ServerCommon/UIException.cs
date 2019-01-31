using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.ServerCommon
{
    public class UIException: Exception
    {
        public UIException(string message) : base(message)
        {
        }
    }
}
