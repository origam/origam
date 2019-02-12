using System.Collections.Generic;

namespace Origam.DA.Service
{
    interface IFileSystemModelChecker
    {
        List<string> GetErrors();
    }
}