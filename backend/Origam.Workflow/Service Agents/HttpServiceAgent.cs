#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Collections;
using System.Xml;
using Origam.Service.Core;

namespace Origam.Workflow;

public class HttpServiceAgent : AbstractServiceAgent
{
    public HttpServiceAgent() { }

    #region IServiceAgent Members

    private object _result;
    public override object Result
    {
        get
        {
            object temp = _result;
            _result = null;

            return temp;
        }
    }

    public override void Run()
    {
        switch (MethodName)
        {
            case "SendRequest":
            case "TrySendRequest":
            {
                HttpResult httpResponse = HttpTools.Instance.SendRequest(
                    request: new Request(
                        url: Parameters.Get<string>(key: "Url"),
                        method: Parameters.Get<string>(key: "Method"),
                        content: GetContent(obj: Parameters[key: "Content"]),
                        contentType: Parameters.TryGet<string>(key: "ContentType"),
                        headers: Parameters[key: "Headers"] as Hashtable,
                        timeout: Parameters.TryGet<int?>(key: "Timeout"),
                        throwExceptionOnError: MethodName == "SendRequest"
                    )
                );
                XmlContainer responseMetadata = Parameters.TryGet<XmlContainer>(
                    key: "ResponseMetadata"
                );
                if (responseMetadata != null)
                {
                    AddMetaData(responseMetadata: responseMetadata, httpResponse: httpResponse);
                }
                _result = httpResponse.Content;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }

    private static void AddMetaData(XmlContainer responseMetadata, HttpResult httpResponse)
    {
        var document = responseMetadata.Xml;
        if (httpResponse.Exception != null)
        {
            XmlElement exceptionElement = document.CreateElement(name: "Exception");
            document.AppendChild(newChild: exceptionElement);

            XmlElement typeElement = document.CreateElement(name: "Type");
            typeElement.InnerText = httpResponse.Exception.GetType().FullName;
            exceptionElement.AppendChild(newChild: typeElement);

            XmlElement messageElement = document.CreateElement(name: "Message");
            messageElement.InnerText = httpResponse.Exception.Message;
            exceptionElement.AppendChild(newChild: messageElement);

            XmlElement stackTraceElement = document.CreateElement(name: "StackTrace");
            stackTraceElement.InnerText = httpResponse.Exception.StackTrace;
            exceptionElement.AppendChild(newChild: stackTraceElement);
        }
        else
        {
            XmlElement httpResponseElement = document.CreateElement(name: "HttpResponse");
            document.AppendChild(newChild: httpResponseElement);

            XmlElement statusCodeElement = document.CreateElement(name: "StatusCode");
            statusCodeElement.InnerText = httpResponse.StatusCode.ToString();
            httpResponseElement.AppendChild(newChild: statusCodeElement);

            XmlElement statusDescriptionElement = document.CreateElement(name: "StatusDescription");
            statusDescriptionElement.InnerText = httpResponse.StatusDescription;
            httpResponseElement.AppendChild(newChild: statusDescriptionElement);

            XmlElement headersElement = document.CreateElement(name: "Headers");
            httpResponseElement.AppendChild(newChild: headersElement);
            foreach (string name in httpResponse.Headers.Keys)
            {
                XmlElement headerElement = document.CreateElement(name: name);
                headerElement.InnerText = httpResponse.Headers[key: name];
                headersElement.AppendChild(newChild: headerElement);
            }
        }
    }

    private string GetContent(object obj)
    {
        if (obj is XmlContainer xmlContainer)
        {
            return xmlContainer.Xml.OuterXml;
        }
        return XmlTools.ConvertToString(val: obj);
    }
    #endregion
}
