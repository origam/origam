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

using System.Collections.Specialized;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Origam.Extensions;

namespace Origam.Mail;

public static class MailLogUtils
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    public static void SendMessageAndLog(SmtpClient client, MailMessage message)
    {
        if (log.IsDebugEnabled)
        {
            log.RunHandled(loggingAction: () =>
            {
                log.Debug(message: "Sending mail:");
                log.Debug(message: MailLogUtils.ToLogString(client: client));
                log.Debug(message: MailLogUtils.ToLogString(message: message));
            });
        }
        client.Send(message: message);
        log.Debug(message: "Mail sent");
    }

    public static string ToLogString(SmtpClient client)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(value: $"System.Net.Mail.SmtpClient:\n");
        builder.Append(value: $"\tHost: {client.Host}\n");
        builder.Append(value: $"\tPort: {client.Port}\n");
        builder.Append(value: $"\tTimeout: {client.Timeout}\n");
        builder.Append(value: $"\tClientCertificates.Count: {client.ClientCertificates.Count}\n");
        builder.Append(value: $"\tDeliveryFormat: {client.DeliveryFormat}\n");
        builder.Append(value: $"\tDeliveryMethod: {client.DeliveryMethod}\n");
        builder.Append(value: $"\tEnableSsl: {client.EnableSsl}\n");
        if (!string.IsNullOrEmpty(value: client.Host))
        {
            builder.Append(value: $"\tServicePoint.Address: {client.ServicePoint.Address}\n");
        }
        builder.Append(value: $"\tTargetName: {client.TargetName}\n");
        builder.Append(value: $"\tPickupDirectoryLocation: {client.PickupDirectoryLocation}\n");
        builder.Append(value: $"\tUseDefaultCredentials: {client.UseDefaultCredentials}\n");
        return builder.ToString();
    }

    public static string ToLogString(MailMessage message)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(value: $"System.Net.Mail.MailMessage:\n");
        builder.Append(value: $"\tSubject: {message.Subject}\n");
        builder.Append(value: $"\tTo: {AddressesToString(mails: message.To)}\n");
        builder.Append(value: $"\tCC: {AddressesToString(mails: message.CC)}\n");
        builder.Append(value: $"\tBcc: {AddressesToString(mails: message.Bcc)}\n");
        builder.Append(value: $"\tReplyToList: {AddressesToString(mails: message.ReplyToList)}\n");
        builder.Append(value: $"\tFrom: {message.From}\n");
        builder.Append(value: $"\tSender: {message.Sender}\n");
        builder.Append(value: $"\tBody:\n {AddIndents(message: message.Body)}\n");
        builder.Append(value: $"\tAttachments.Count: {message.Attachments.Count}\n");
        builder.Append(value: $"\tIsBodyHtml: {message.IsBodyHtml}\n");
        builder.Append(value: $"\tBodyEncoding: {message.BodyEncoding}\n");
        builder.Append(value: $"\tBodyTransferEncoding: {message.BodyTransferEncoding}\n");
        builder.Append(value: $"\tHeaders: {HeadersToString(headers: message.Headers)}\n");
        builder.Append(value: $"\tHeadersEncoding: {message.HeadersEncoding}\n");
        builder.Append(value: $"\tSubjectEncoding: {message.SubjectEncoding}\n");
        builder.Append(value: $"\tPriority: {message.Priority}\n");
        builder.Append(value: $"\tAlternateViews.Count: {message.AlternateViews.Count}\n");
        builder.Append(
            value: $"\tDeliveryNotificationOptions: {message.DeliveryNotificationOptions}\n"
        );
        return builder.ToString();
    }

    private static string HeadersToString(NameValueCollection headers)
    {
        return $"Headers: [{string.Join(separator: ", ", values: headers.AllKeys.Select(selector: key => key + ": " + headers[name: key]))}]";
    }

    private static string AddressesToString(MailAddressCollection mails)
    {
        return $"[{string.Join(separator: ", ", values: mails.Select(selector: x => x.Address))}]";
    }

    private static string AddIndents(string message)
    {
        return string.Join(
            separator: "\n",
            values: message.Split(separator: '\n').Select(selector: x => "\t\t" + x)
        );
    }
}
