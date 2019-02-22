using System.Collections.Generic;
using Origam.DA.Service.FileSystemModeCheckers;

namespace Origam.DA.Service
{
    interface IFileSystemModelChecker
    {
        ModelErrorSection GetErrors();
    }
}