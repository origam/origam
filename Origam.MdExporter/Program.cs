using Origam.DA;
using Origam.Schema;
using Origam.Workbench.Services;
using System;
using System.Collections.Generic;

namespace Origam.MdExporter
{
    class Program
    {
        static void Main(string[] args)
        {

            var DefaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };

            string testFolderPath = "";
            var persistenceService = new FilePersistenceService(DefaultFolders,
                testFolderPath);

        }
    }
}
