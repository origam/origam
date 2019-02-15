using System;
using System.IO;
using System.Text;
using System.Web;
using Origam.ServerCommon.Pages;

namespace Origam.Server.Pages
{
    internal class FxHttpResponseWrapper : IResponseWrapper
    {
        private readonly HttpResponse response;

        public FxHttpResponseWrapper(HttpResponse response)
        {
            this.response = response;
            response.ContentEncoding = Encoding.UTF8;
        }

        public bool BufferOutput
        {
            get => response.BufferOutput;
            set => response.BufferOutput = value;
        } 
        public string ContentType
        {
            get => response.ContentType;
            set => response.ContentType = value;
        }

        public bool TrySkipIisCustomErrors
        {
            get => response.TrySkipIisCustomErrors;
            set => response.TrySkipIisCustomErrors = value;
        }

        public int StatusCode
        {
            get => response.StatusCode;
            set => response.StatusCode = value;
        }

        public void WriteToOutput(Action<TextWriter> writeAction)
        {
            writeAction(response.Output);
        }

        public void CacheSetMaxAge(TimeSpan timeSpan)
        {
            response.Cache.SetMaxAge(timeSpan);
        }

        public void End()
        {
            response.End();
        }

        public void Clear()
        {
            response.Clear();
        }

        public void Write(string message)
        {
            response.Write(message);
        }

        public void AddHeader(string name, string value)
        {
            response.AddHeader(name, value);
        }

        public void BinaryWrite(byte[] bytes)
        {
            response.BinaryWrite(bytes);
        }

        public void Redirect(string requestUrlReferrerAbsolutePath)
        {
            response.Redirect(requestUrlReferrerAbsolutePath);
        }

        public void OutputStreamWrite(byte[] buffer, int offset, int count)
        {
            response.OutputStream.Write(buffer, offset, count);
        }
    }
}