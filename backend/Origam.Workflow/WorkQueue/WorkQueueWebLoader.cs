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
using System.Data;
using System.Net;
using System.IO;
using System.Xml;

using Origam.Workbench.Services;
using System.Collections;
using System.Xml.XPath;

namespace Origam.Workflow.WorkQueue
{
	/// <summary>
	/// Summary description for WorkQueueFileLoader.
	/// </summary>
	public class WorkQueueWebLoader : WorkQueueLoaderAdapter
	{
		string _url = null;
        string _stateXPath = null;
		bool _executed = false;
        string _authentication = null;
        string _userName = null;
        string _password = null;

		public override void Connect(
            IWorkQueueService service, Guid queueId, string workQueueClass, 
            string connection, string userName, string password, 
            string transactionId)
		{
			string[] cnParts = connection.Split(";".ToCharArray());
			foreach (string part in cnParts)
			{
				string[] pair = part.Split("=".ToCharArray(), 2);
				if (pair.Length == 2)
				{
					switch (pair[0])
					{
						case "url":
							_url = pair[1];
							break;
                        case "stateXPath":
                            _stateXPath = pair[1];
                            break;
						default:
							throw new ArgumentOutOfRangeException(
                                "connectionParameterName", pair[0], 
                                ResourceUtils.GetString(
                                    "ErrorInvalidConnectionString"));
					}
				}
			}
            _userName = userName;
            _password = password;
			if(_url == null || _url == string.Empty)
			{
				throw new Exception("url parameter not specified in the connection string.");
			}
		}

		public override void Disconnect()
		{
		}

		public override WorkQueueAdapterResult GetItem(string lastState)
		{
            if(_executed)
            {
                return null;
            }
			_executed = true;
            if(!String.IsNullOrEmpty(_userName))
            {
                _authentication = "Basic";
            }
            string url = _url.Replace("{lastState}", lastState);
            using (WebResponse response = HttpTools.Instance.GetResponse(
                new Request(
                    url: url,
                    method: null,
                    authenticationType: _authentication,
                    userName: _userName,
                    password: _password)
                )
            )
            {
                HttpWebResponse httpResponse = response as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                WorkQueueFileLoader.FileType mode;
                if (response.ContentType.ToLower().StartsWith("text/")
                || response.ContentType.ToLower() == "application/json"
                || response.ContentType.ToLower() == "application/xml")
                {
                    mode = WorkQueueFileLoader.FileType.TEXT;
                }
                else
                {
                    mode = WorkQueueFileLoader.FileType.BINARY;
                }
                DataSet dataset = WorkQueueFileLoader.GetFileFromStream(
                    responseStream, mode, response.ResponseUri.AbsoluteUri,
                    response.ResponseUri.AbsoluteUri,
                    HttpTools.Instance.EncodingFromResponse(httpResponse).WebName);
                WorkQueueAdapterResult result = new WorkQueueAdapterResult(
                    DataDocumentFactory.New(dataset));
                if (!String.IsNullOrEmpty(_stateXPath))
                {
                    result.State = GetNewStateViaXPath(dataset, lastState);
                }
                return result;
            }
		}

        private string GetNewStateViaXPath(DataSet dataset, string lastState)
        {
            XPathDocument xPathDocument = new XPathDocument(
                new StringReader(dataset.Tables["File"].Rows[0]["Data"]
                .ToString()));
            XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
            string newState = xPathNavigator.Evaluate(_stateXPath)?.ToString();
            if(int.TryParse(newState, out int number))
            {
                return newState;
            }
            else
            {
                return lastState;
            }
        }
	}
}
