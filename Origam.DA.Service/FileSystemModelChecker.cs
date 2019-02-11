using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Origam.Extensions;

namespace Origam.DA.Service
{
    class FileSystemModelChecker
    {
        private readonly DirectoryInfo topDirectory;
        private readonly string[] ignoreDirectoryNames;

        public FileSystemModelChecker(DirectoryInfo topDirectory, string[] ignoreDirectoryNames)
        {
            this.topDirectory = topDirectory;
            this.ignoreDirectoryNames = ignoreDirectoryNames;
        }

        public List<string> GetFileErrors()
        {
            DirectoryInfo[] packageDirectories = topDirectory.GetDirectories()
                    .Where(dir => !ignoreDirectoryNames.Contains(dir.Name))
                    .ToArray();
            IEnumerable<DirectoryInfo> packageSubDirectories = packageDirectories
                .SelectMany(dir => dir.GetDirectories())
                .Where(dir => !ignoreDirectoryNames.Contains(dir.Name))
                .ToArray();
            IEnumerable<DirectoryInfo> groupDirectories = packageSubDirectories
                .SelectMany(dir => dir.GetDirectories())
                .Where(dir => !ignoreDirectoryNames.Contains(dir.Name));

            List<string> errors = new List<string>();
            errors.AddRange(FindErrorsInPackageDirectories(packageDirectories));
            errors.AddRange(FindErrorsInPackageSubDirectories(packageSubDirectories));
            errors.AddRange(FindErrorsInGroupDirectories(groupDirectories));
            return errors;
        }

        private IEnumerable<string> FindErrorsInGroupDirectories(IEnumerable<DirectoryInfo> groupDirectories)
        {
            return groupDirectories
                .Where(dir =>
                       dir.GetFiles().Length != 0
                    && dir.DoesNotContain(OrigamFile.ReferenceFileName)
                    && dir.DoesNotContain(OrigamFile.GroupFileName)
                    && ContainsOrigamFilesWithMissingExplicitParent(dir))
                .Select(dir => dir.FullName + " is a group directory and therefore must contain either " +
                               OrigamFile.ReferenceFileName + ", or " + OrigamFile.GroupFileName);
        }

        private IEnumerable<string> FindErrorsInPackageDirectories(DirectoryInfo[] packageDirectories)
        {
            var packageFilesMissing = packageDirectories
                .Where(dir => dir.DoesNotContain(OrigamFile.PackageFileName))
                .Select(dir => dir.FullName + " is a package directory but the package file " + OrigamFile.PackageFileName + " is not in it");

            var wrongFilesPresent = packageDirectories
                .Where(dir => 
                    dir.Contains(OrigamFile.GroupFileName)
                    || dir.Contains(OrigamFile.ReferenceFileName)
                    || dir.Contains(file => OrigamFile.IsOrigamFile(file)))
                .Select(dir => dir.FullName + " is a package directory and therefore cannot contain any persistence files other than " + OrigamFile.PackageFileName);

            return packageFilesMissing.Concat(wrongFilesPresent);
        }

        private IEnumerable<string> FindErrorsInPackageSubDirectories(IEnumerable<DirectoryInfo> packageSubDirectories)
        {
            return packageSubDirectories
                    .Where(dir => dir.Contains(OrigamFile.GroupFileName)
                               || dir.Contains(OrigamFile.ReferenceFileName)
                               || dir.Contains(OrigamFile.PackageFileName))
                    .Select(dir => dir.FullName + " is a package sub directory and therefore cannot contain any of the following files: " +
                            OrigamFile.PackageFileName + ", " + OrigamFile.ReferenceFileName + ", or " + OrigamFile.GroupFileName);
        }

        private static bool ContainsOrigamFilesWithMissingExplicitParent(DirectoryInfo directory)
        {
            return directory
                .GetFiles()
                .Where(OrigamFile.IsOrigamFile)
                .Any(ContainsItemWithMissingReferenceToParentObject);
        }

        private static bool ContainsItemWithMissingReferenceToParentObject(FileInfo origamFile)
        {
            var document = new XmlDocument();
            try
            {
                document.LoadXml(File.ReadAllText(origamFile.FullName));
            }
            catch (Exception e) when (e is FileNotFoundException || e is XmlException)
            {
                return false;
            }

            var topLevelNodes = document.ChildNodes[1]?.ChildNodes;
            return topLevelNodes
                       ?.Cast<XmlNode>()
                       .Any(node =>
                       {
                           var xmlAttribute = node.Attributes?[$"x:{OrigamFile.ParentIdAttribute}"];
                           if (xmlAttribute == null) return true;
                           bool parseSuccess = Guid.TryParse(xmlAttribute.Value, out Guid parentId);
                           if (!parseSuccess) return true;
                           return parentId == Guid.Empty;
                       })
                   ?? false;
        }
    }
}