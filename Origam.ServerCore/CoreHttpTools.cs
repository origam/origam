using Origam.ServerCommon;
using Origam.ServerCommon.Pages;
using System.Net;

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
            bool isFirefox 
                = (request.UserAgent != null) 
                && (request.UserAgent.IndexOf("Firefox") >= 0);
            string dispositionLeft = "filename=";
            if(isFirefox)
            {
                dispositionLeft = "filename*=utf-8''";
            }
            // no commas allowed in the file name
            fileName = fileName.Replace(",", "");
            string disposition = dispositionLeft + WebUtility.UrlEncode(fileName);
            return disposition;
        }
    }
}