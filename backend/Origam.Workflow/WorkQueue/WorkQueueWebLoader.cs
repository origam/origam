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
using System.IO;
using System.Net;
using System.Xml.XPath;
using Origam.Workbench.Services;

namespace Origam.Workflow.WorkQueue;

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
        IWorkQueueService service,
        Guid queueId,
        string workQueueClass,
        string connection,
        string userName,
        string password,
        string transactionId
    )
    {
        string[] cnParts = connection.Split(separator: ";".ToCharArray());
        foreach (string part in cnParts)
        {
            string[] pair = part.Split(separator: "=".ToCharArray(), count: 2);
            if (pair.Length == 2)
            {
                switch (pair[0])
                {
                    case "url":
                    {
                        _url = pair[1];
                        break;
                    }

                    case "stateXPath":
                    {
                        _stateXPath = pair[1];
                        break;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "connectionParameterName",
                            actualValue: pair[0],
                            message: ResourceUtils.GetString(key: "ErrorInvalidConnectionString")
                        );
                    }
                }
            }
        }
        _userName = userName;
        _password = password;
        if (_url == null || _url == string.Empty)
        {
            throw new Exception(message: "url parameter not specified in the connection string.");
        }
    }

    public override void Disconnect() { }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        if (_executed)
        {
            return null;
        }
        _executed = true;
        if (!String.IsNullOrEmpty(value: _userName))
        {
            _authentication = "Basic";
        }
        string url = _url.Replace(oldValue: "{lastState}", newValue: lastState);
        using (
            WebResponse response = HttpTools.Instance.GetResponse(
                request: new Request(
                    url: url,
                    method: null,
                    authenticationType: _authentication,
                    userName: _userName,
                    password: _password
                )
            )
        )
        {
            HttpWebResponse httpResponse = response as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            WorkQueueFileLoader.FileType mode;
            if (
                response.ContentType.ToLower().StartsWith(value: "text/")
                || response.ContentType.ToLower() == "application/json"
                || response.ContentType.ToLower() == "application/xml"
            )
            {
                mode = WorkQueueFileLoader.FileType.TEXT;
            }
            else
            {
                mode = WorkQueueFileLoader.FileType.BINARY;
            }
            DataSet dataset = WorkQueueFileLoader.GetFileFromStream(
                stream: responseStream,
                mode: mode,
                filename: response.ResponseUri.AbsoluteUri,
                title: response.ResponseUri.AbsoluteUri,
                encoding: HttpTools.Instance.EncodingFromResponse(response: httpResponse).WebName
            );
            WorkQueueAdapterResult result = new WorkQueueAdapterResult(
                document: DataDocumentFactory.New(dataSet: dataset)
            );
            if (!String.IsNullOrEmpty(value: _stateXPath))
            {
                result.State = GetNewStateViaXPath(dataset: dataset, lastState: lastState);
            }
            return result;
        }
    }

    private string GetNewStateViaXPath(DataSet dataset, string lastState)
    {
        XPathDocument xPathDocument = new XPathDocument(
            textReader: new StringReader(
                s: dataset.Tables[name: "File"].Rows[index: 0][columnName: "Data"].ToString()
            )
        );
        XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
        string newState = xPathNavigator.Evaluate(xpath: _stateXPath)?.ToString();
        if (int.TryParse(s: newState, result: out int number))
        {
            return newState;
        }

        return lastState;
    }
}
