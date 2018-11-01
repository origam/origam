#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

namespace Origam.Workflow.WorkQueue
{
	/// <summary>
	/// Summary description for WorkQueueFileLoader.
	/// </summary>
	public class WorkQueueWebLoader : WorkQueueLoaderAdapter
	{
		string _url = null;
		bool _executed = false;

		public WorkQueueWebLoader()
		{
		}

		public override void Connect(IWorkQueueService service, Guid queueId, string workQueueClass, string connection, string userName, string password, string transactionId)
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
						default:
							throw new ArgumentOutOfRangeException("connectionParameterName", pair[0], ResourceUtils.GetString("ErrorInvalidConnectionString"));
					}
				}
			}

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
			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
			if(_executed) return null;
			_executed = true;

			WebRequest request = HttpWebRequest.Create(_url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			Stream responseStream = response.GetResponseStream();

			string mode;
			if(response.ContentType.ToLower().StartsWith("text/") 
				|| response.ContentType.ToLower() == "application/json"
				|| response.ContentType.ToLower() == "application/xml")
			{
                mode = WorkQueueFileLoader.MODE_TEXT;
			}
			else
			{
                mode = WorkQueueFileLoader.MODE_BINARY;
			}

			DataSet ds = WorkQueueFileLoader.GetFileFromStream(responseStream, mode, response.ResponseUri.AbsoluteUri,
                response.ResponseUri.AbsoluteUri, HttpTools.EncodingFromResponse(response).WebName);

			return new WorkQueueAdapterResult(DataDocumentFactory.New(ds));
		}

	}
}
