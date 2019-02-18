using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

namespace Origam.ServerCommon.Pages
{
    internal class StandardHttpResponseWrapper : IResponseWrapper
    {
        private readonly HttpContext httpContext;
        private readonly HttpResponse response;
        private readonly Encoding encoding;

        public StandardHttpResponseWrapper(HttpContext httpContext)
        {
            this.httpContext = httpContext;
            this.response = httpContext.Response;
            encoding = Encoding.UTF8;
        }

        public bool BufferOutput
        {
            set
            {
                if (!value)
                {
                    httpContext.Features.Get<IHttpBufferingFeature>()?.DisableResponseBuffering();
                }
            }
        }

        public string ContentType
        {
            set
            {
                var mediaType = new MediaTypeHeaderValue(value);
                mediaType.Encoding = encoding;
                response.ContentType = mediaType.ToString();
            }
        }


        public bool TrySkipIisCustomErrors
        {
            set { } // probably not necessary https://stackoverflow.com/questions/49269381/response-tryskipiiscustomerrors-equivalent-for-asp-net-core
        }

        public int StatusCode
        {
            set => response.StatusCode = value;
        }

        public void WriteToOutput(Action<TextWriter> writeAction)
        {
            TextWriter textWriter = new StringWriter();
            writeAction(textWriter);
            response.WriteAsync(textWriter.ToString()).Wait();
        }

        public void CacheSetMaxAge(TimeSpan timeSpan)
        {
            response.Headers[HeaderNames.CacheControl] = "max-age=" + timeSpan.TotalSeconds;
        }

        public void End()
        {
        }

        public void Clear()
        {
            response.Clear();
        }

        public void Write(string message)
        {
            response.WriteAsync(message).Wait();
        }

        public void AddHeader(string name, string value)
        {
            response.Headers[name]= value;
        }

        public void BinaryWrite(byte[] bytes)
        {
            response.Body.WriteAsync(bytes,0, bytes.Length).Wait();
        }

        public void Redirect(string requestUrlReferrerAbsolutePath)
        {
            response.Redirect(requestUrlReferrerAbsolutePath);
        }

        public void OutputStreamWrite(byte[] buffer, int offset, int count)
        {
            response.Body.WriteAsync(buffer, offset, count).Wait();
        }
    }
}