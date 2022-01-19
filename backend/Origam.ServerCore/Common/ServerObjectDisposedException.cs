using System;

namespace Origam.ServerCommon
{
    public class ServerObjectDisposedException: Exception
    {
        public ServerObjectDisposedException(string message) : base(message)
        {
        }
    }
}