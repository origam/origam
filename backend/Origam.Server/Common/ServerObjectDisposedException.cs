using System;

namespace Origam.Server;

public class ServerObjectDisposedException: Exception
{
    public ServerObjectDisposedException(string message) : base(message)
    {
        }
}