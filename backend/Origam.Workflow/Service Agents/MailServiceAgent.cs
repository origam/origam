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
            {
                // Check input parameters
                if (!(this.Parameters[key: "Data"] is IXmlContainer))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotXmlDocument")
                    );
                }

                string server = null;
                int port = 25;
                if (this.Parameters.Contains(key: "Server"))
                {
                    if (!(this.Parameters[key: "Server"] is string))
                    {
                        throw new InvalidCastException(
                            message: ResourceUtils.GetString(key: "ErrorServerNotString")
                        );
                    }

                    server = this.Parameters[key: "Server"] as String;
                }
                if (this.Parameters.Contains(key: "Port"))
                {
                    if (!(this.Parameters[key: "Port"] is int))
                    {
                        throw new InvalidCastException(
                            message: ResourceUtils.GetString(key: "ErrorPortNotInt")
                        );
                    }

                    port = Convert.ToInt32(value: this.Parameters[key: "Port"]);
                }
                _result = MailServiceFactory
                    .GetMailService()
                    .SendMail(
                        mailDocument: this.Parameters[key: "Data"] as IXmlContainer,
                        server: server,
                        port: port
                    );

                break;
            }

            case "RetrieveMails":
            {
                // Check input parameters
                if (!(this.Parameters[key: "UserName"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorMailServerNotString")
                    );
                }

                if (!(this.Parameters[key: "Password"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPasswordNotString")
                    );
                }

                if (!(this.Parameters[key: "Server"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorServerNotString")
                    );
                }

                if (!(this.Parameters[key: "Port"] is int))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPortNotInt")
                    );
                }

                _result = AbstractMailService.GetMails(
                    mailServer: this.Parameters[key: "Server"] as String,
                    port: Convert.ToInt32(value: this.Parameters[key: "Port"]),
                    userName: this.Parameters[key: "UserName"] as String,
                    password: this.Parameters[key: "Password"] as String,
                    transactionId: this.TransactionId,
                    maxCount: 0
                );

                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: this.MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }
    #endregion
}
