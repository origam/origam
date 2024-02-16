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

namespace Origam.Workflow
{
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
			switch(MethodName)
			{
				case "SendRequest":
                    HttpResponse httpResponse = 
						HttpTools.Instance.SendRequest(
							url: Parameters.Get<string>("Url"),
							method: Parameters.Get<string>("Method"),
							content: GetContent(Parameters["Content"]),
							contentType: Parameters.TryGet<string>("ContentType"),
							headers: Parameters["Headers"] as Hashtable,
							timeout: Parameters.TryGet<int?>("Timeout")
						);
                    XmlContainer responseMetadata = Parameters.TryGet<XmlContainer>("ResponseMetadata");
					if (responseMetadata != null)
                    {
                        AddMetaData(responseMetadata, httpResponse);
                    }
                    _result = httpResponse.Content;
					break;
				default:
					throw new ArgumentOutOfRangeException("MethodName", MethodName, ResourceUtils.GetString("InvalidMethodName"));
			}
		}

        private static void AddMetaData(XmlContainer responseMetadata, HttpResponse httpResponse)
        {
            var document = responseMetadata.Xml;

            XmlElement httpResponseNode = document.CreateElement("HttpResponse");
            document.AppendChild(httpResponseNode);

            XmlElement statusCodeNode = document.CreateElement("StatusCode");
            statusCodeNode.InnerText = httpResponse.StatusCode.ToString();
            httpResponseNode.AppendChild(statusCodeNode);

            XmlElement statusDescriptionNode = document.CreateElement("StatusDescription");
            statusDescriptionNode.InnerText = httpResponse.StatusDescription;
            httpResponseNode.AppendChild(statusDescriptionNode);

            XmlElement headersNode = document.CreateElement("Headers");
            httpResponseNode.AppendChild(headersNode);
            foreach (string name in httpResponse.Headers.Keys)
            {
                XmlElement headerNode = document.CreateElement(name);
                headerNode.InnerText = httpResponse.Headers[name];
                headersNode.AppendChild(headerNode);
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
}
