using System.IO;
using CSharpFunctionalExtensions;
using Origam.Workbench.Services;

namespace Origam.Services
{
    public interface IFileStorageDocumentationService: IDocumentationService
    {
        Maybe<string> GetDocumentationFileHash(FileInfo filePath);
    }
}