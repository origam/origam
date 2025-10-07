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
using System.Xml;
using Origam.Mail;
using Origam.Service.Core;

namespace Origam.Workflow;

/// <summary>
/// Summary description for DataService.
/// </summary>
public class MailServiceAgent : AbstractServiceAgent
{
    public MailServiceAgent() { }

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
        switch (this.MethodName)
        {
            case "SendMail":
                // Check input parameters
                if (!(this.Parameters["Data"] is IXmlContainer))
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorNotXmlDocument"));
                string server = null;
                int port = 25;
                if (this.Parameters.Contains("Server"))
                {
                    if (!(this.Parameters["Server"] is string))
                        throw new InvalidCastException(
                            ResourceUtils.GetString("ErrorServerNotString")
                        );
                    server = this.Parameters["Server"] as String;
                }
                if (this.Parameters.Contains("Port"))
                {
                    if (!(this.Parameters["Port"] is int))
                        throw new InvalidCastException(ResourceUtils.GetString("ErrorPortNotInt"));
                    port = Convert.ToInt32(this.Parameters["Port"]);
                }
                _result = MailServiceFactory
                    .GetMailService()
                    .SendMail(this.Parameters["Data"] as IXmlContainer, server, port);

                break;
            case "RetrieveMails":
                // Check input parameters
                if (!(this.Parameters["UserName"] is string))
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorMailServerNotString")
                    );
                if (!(this.Parameters["Password"] is string))
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorPasswordNotString")
                    );
                if (!(this.Parameters["Server"] is string))
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorServerNotString"));
                if (!(this.Parameters["Port"] is int))
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPortNotInt"));
                _result = AbstractMailService.GetMails(
                    this.Parameters["Server"] as String,
                    Convert.ToInt32(this.Parameters["Port"]),
                    this.Parameters["UserName"] as String,
                    this.Parameters["Password"] as String,
                    this.TransactionId,
                    0
                );

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "MethodName",
                    this.MethodName,
                    ResourceUtils.GetString("InvalidMethodName")
                );
        }
    }
    #endregion
}
