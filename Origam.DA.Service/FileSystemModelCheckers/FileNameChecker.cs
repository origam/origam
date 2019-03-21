using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.FileSystemModeCheckers;
using Origam.Schema;

namespace Origam.DA.Service
{
    public class FileNameChecker : IFileSystemModelChecker
    {
        private readonly FilePersistenceProvider filePersistenceProvider;
        private readonly FilePersistenceIndex index;
        private readonly string topDirectory;

        public FileNameChecker(FilePersistenceProvider filePersistenceProvider,
            FilePersistenceIndex index)
        {
            this.filePersistenceProvider = filePersistenceProvider;
            this.index = index;
            topDirectory = this.filePersistenceProvider.TopDirectory.FullName;
        }

        public ModelErrorSection GetErrors()
        {
            IFilePersistent[] allPersistedObjects = filePersistenceProvider
                .RetrieveList<IFilePersistent>()
                .ToArray();

            HashSet<Guid> allChildrenIds = allPersistedObjects
                .OfType<AbstractSchemaItem>()
                .SelectMany(GetChildrenIds)
                .ToHashSet();

            List<string> errors = allPersistedObjects
                .Where(instance => !(instance is SchemaItemAncestor))
                .Where(instance => !allChildrenIds.Contains(instance.Id))
                .Where(IsPersistedInWrongFile)
                .Select(instance =>
                {
                    string actualFilePath = index.GetById(instance.Id).OrigamFile.Path.Absolute;
                    string expectedFilePath = Path.Combine(topDirectory,instance.RelativeFilePath);
                    string expectedFilePathFormatted = File.Exists(expectedFilePath)
                        ? $"file://{expectedFilePath}"
                        : expectedFilePath;
                    return $"Object with id: \"{instance.Id}\"\n" +
                           $"should be in: \"{expectedFilePathFormatted}\"\n" +
                           $"but is in: \"file://{actualFilePath}\"";
                })
                .ToList();

            return new ModelErrorSection("Objects persisted in wrong files (object name is different from file name)", errors);
        }

        private IEnumerable<Guid> GetChildrenIds(AbstractSchemaItem item)
        {
            return item.ChildItems
                .ToEnumerable()
                .Select(x=>x.Id);
        }

        private bool IsPersistedInWrongFile(IFilePersistent instance)
        {
            var objectInfo = index.GetById(instance.Id);
            return objectInfo.OrigamFile.Path.Relative != instance.RelativeFilePath;
        }
    }
}