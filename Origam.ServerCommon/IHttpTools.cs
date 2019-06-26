
using Origam.ServerCommon.Pages;

namespace Origam.ServerCommon
{
    public interface IHttpTools
    {
        void WriteFile(IRequestWrapper request, IResponseWrapper response, byte[] file, string fileName, bool isPreview);
        void WriteFile(IRequestWrapper request, IResponseWrapper response, byte[] file, string fileName, bool isPreview, string overrideContentType);
        string GetFileDisposition(IRequestWrapper request, string fileName);
    }
}