//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using Origam.Extensions;
//
//namespace Origam.DA.Service
//{
//    class FileSystemModelChecker
//    {
//        private readonly DirectoryInfo topDirectory;
//
//        public FileSystemModelChecker(DirectoryInfo topDirectory)
//        {
//            this.topDirectory = topDirectory;
//        }
//
//        public List<string> GetFileErrors()
//        {
//            DirectoryInfo[] packageDirectories = topDirectory.GetDirectories();
//            IEnumerable<DirectoryInfo> packageSubDirectories = packageDirectories
//                .SelectMany(dir => dir.GetDirectories())
//                .ToArray();
//            IEnumerable<DirectoryInfo> groupDirectories = packageSubDirectories
//                .SelectMany(dir => dir.GetDirectories());
//
//            List<string> errors = new List<string>();
//            errors.AddRange(FindErrorsInPackageDirectories(packageDirectories));
//            errors.AddRange(FindErrorsInPackageSubDirectories(packageSubDirectories));
//            errors.AddRange(FindErrorsInGroupDirectories(groupDirectories));
//            return errors;
//        }
//
//        private IEnumerable<string> FindErrorsInGroupDirectories(IEnumerable<DirectoryInfo> groupDirectories)
//        {
//            return groupDirectories
//                .Where(dir =>
//                    dir.DoesNotContain(OrigamFile.ReferenceFileName)
//                    && dir.DoesNotContain(OrigamFile.GroupFileName))
//                .Select(dir => dir.FullName + " is a group directory and therefore must contain either " +
//                               OrigamFile.ReferenceFileName + ", or " + OrigamFile.GroupFileName);
//        }
//
//        private IEnumerable<string> FindErrorsInPackageDirectories(IEnumerable<DirectoryInfo> packageDirectories)
//        {
//            return packageDirectories
//                .Where(dir => dir.DoesNotContain(OrigamFile.PackageFileName))
//                .Select(dir => dir.FullName + " is a package directory but the package file " + OrigamFile.PackageFileName + " is not in it");
//        }
//
//        private IEnumerable<string> FindErrorsInPackageSubDirectories(IEnumerable<DirectoryInfo> packageSubDirectories)
//        {
//            return packageSubDirectories
//                    .Where(dir =>
//                        dir.Contains(OrigamFile.PackageFileName)
//                        || dir.Contains(OrigamFile.ReferenceFileName)
//                        || dir.Contains(OrigamFile.GroupFileName))
//                    .Select(dir => dir.FullName + " is a package sub directory and therefore cannot contain " +
//                            OrigamFile.PackageFileName + ", or " + OrigamFile.ReferenceFileName + ", or " + OrigamFile.GroupFileName);
//        }
//    }
//}