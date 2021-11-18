#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

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

        public string Charset
        {
            set => response.Charset = value;
            get => response.Charset;
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

        public void AppendHeader(string contentDisposition, string disposition)
        {
            response.AppendHeader(contentDisposition,disposition);
        }
    }
}