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
    public HttpServiceAgent()
    {
    }

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
                    HttpResult httpResponse =
                        HttpTools.Instance.SendRequest(
                            new Request(
                                url: Parameters.Get<string>("Url"),
                                method: Parameters.Get<string>("Method"),
                                content: GetContent(Parameters["Content"]),
                                contentType: Parameters.TryGet<string>("ContentType"),
                                headers: Parameters["Headers"] as Hashtable,
                                timeout: Parameters.TryGet<int?>("Timeout"),
                                throwExceptionOnError: MethodName == "SendRequest"
                            )
                        );
                    XmlContainer responseMetadata = Parameters
                            .TryGet<XmlContainer>("ResponseMetadata");
                    if (responseMetadata != null)
                    {
                        AddMetaData(responseMetadata, httpResponse);
                    }
                    _result = httpResponse.Content;
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(
                "MethodName", MethodName, ResourceUtils.GetString("InvalidMethodName"));
        }
    }

    private static void AddMetaData(XmlContainer responseMetadata, HttpResult httpResponse)
    {
        var document = responseMetadata.Xml;
        if (httpResponse.Exception != null)
        {
            XmlElement exceptionElement = document.CreateElement("Exception");
            document.AppendChild(exceptionElement);

            XmlElement typeElement = document.CreateElement("Type");
            typeElement.InnerText = httpResponse.Exception.GetType().FullName;
            exceptionElement.AppendChild(typeElement);

            XmlElement messageElement = document.CreateElement("Message");
            messageElement.InnerText = httpResponse.Exception.Message;
            exceptionElement.AppendChild(messageElement);

            XmlElement stackTraceElement = document.CreateElement("StackTrace");
            stackTraceElement.InnerText = httpResponse.Exception.StackTrace;
            exceptionElement.AppendChild(stackTraceElement);
        }
        else
        {
            XmlElement httpResponseElement = document.CreateElement("HttpResponse");
            document.AppendChild(httpResponseElement);

            XmlElement statusCodeElement = document.CreateElement("StatusCode");
            statusCodeElement.InnerText = httpResponse.StatusCode.ToString();
            httpResponseElement.AppendChild(statusCodeElement);

            XmlElement statusDescriptionElement = document.CreateElement("StatusDescription");
            statusDescriptionElement.InnerText = httpResponse.StatusDescription;
            httpResponseElement.AppendChild(statusDescriptionElement);

            XmlElement headersElement = document.CreateElement("Headers");
            httpResponseElement.AppendChild(headersElement);
            foreach (string name in httpResponse.Headers.Keys)
            {
                XmlElement headerElement = document.CreateElement(name);
                headerElement.InnerText = httpResponse.Headers[name];
                headersElement.AppendChild(headerElement);
            }
        }
    }

    private string GetContent(object obj)
    {
        if (obj is XmlContainer xmlContainer)
        {
            return xmlContainer.Xml.OuterXml;
        }
        return XmlTools.ConvertToString(obj);
    }
    #endregion
}

