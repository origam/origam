using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.ServerCommon
{
    class UIException: Exception
    {
        public UIException(string message) : base(message)
        {
        }
    }
}
