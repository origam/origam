using Origam.ServerCommon;
using Origam.ServerCommon.Pages;

namespace Origam.ServerCore
{
    public class CoreHttpTools : IHttpTools
    {
        public void WriteFile(IRequestWrapper request, IResponseWrapper response, byte[] file,
            string fileName, bool isPreview)
        {
            throw new System.NotImplementedException();
        }

        public void WriteFile(IRequestWrapper request, IResponseWrapper response, byte[] file,
            string fileName, bool isPreview, string overrideContentType)
        {
            throw new System.NotImplementedException();
        }

        public string GetFileDisposition(IRequestWrapper request, string fileName)
        {
            throw new System.NotImplementedException();
        }
    }
}