using System.Text;
using System.Web;

namespace Origam.Server
{
    public class NetFxHttpTools
    {
        public static void WriteFile(HttpRequest request, HttpResponse response, byte[] file, string fileName, bool isPreview)
        {
            WriteFile(request, response, file, fileName, isPreview, null);
        }

        public static void WriteFile(HttpRequest request, HttpResponse response, byte[] file, string fileName, bool isPreview, string overrideContentType)
        {
            // set proper content type
            if (overrideContentType != null)
            {
                response.ContentType = overrideContentType;
            }
            else
            {
                response.ContentType = Origam.HttpTools.GetMimeType(fileName);
            }
            response.Charset = Encoding.UTF8.WebName;
            string disposition = GetFileDisposition(request, fileName);
            if (!isPreview) disposition = "attachment; " + disposition;
            response.AppendHeader("content-disposition", disposition);
            // write to response.OutputStream
            response.OutputStream.Write(file, 0, file.Length);
        }
        
        public static string GetFileDisposition(HttpRequest request, string fileName)
        {
            bool isFirefox = request.UserAgent != null && request.UserAgent.IndexOf("Firefox") >= 0;

            string dispositionLeft = "filename=";
            if (isFirefox)
            {
                dispositionLeft = "filename*=utf-8''";
            }

            // no commas allowed in the file name
            fileName = fileName.Replace(",", "");

            string disposition = dispositionLeft + HttpUtility.UrlPathEncode(fileName);

            return disposition;
        }

    }
}