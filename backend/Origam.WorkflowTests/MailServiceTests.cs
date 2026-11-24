#region license

/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using MimeKit;
using Moq;
using NUnit.Framework;
using Origam.Mail;
using Origam.Service.Core;

namespace Origam.WorkflowTests;

[TestFixture]
public class MailServiceTests
{
    private string pickupDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        pickupDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(pickupDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(pickupDirectory, recursive: true);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("Support <support@example.com>")]
    public void ShouldReadReplyToFromMailData(string? replyTo)
    {
        var data = CreateMailData(replyTo);
        SendMail(data);

        using var message = MimeMessage.Load(Directory.GetFiles(pickupDirectory).Single());
        Assert.That(message.From.Mailboxes.Single().Address, Is.EqualTo("sender@example.com"));
        if (string.IsNullOrWhiteSpace(replyTo))
        {
            Assert.That(message.ReplyTo, Is.Empty);
        }
        else
        {
            var address = message.ReplyTo.Mailboxes.Single();
            Assert.That(address.Address, Is.EqualTo("support@example.com"));
            Assert.That(address.Name, Is.EqualTo("Support"));
        }
    }

    [Test]
    public void ShouldUseEachMailsOwnReplyTo()
    {
        var data = CreateMailData("first@example.com");
        data.Merge(CreateMailData("second@example.com"));

        SendMail(data);

        var addresses = Directory
            .GetFiles(pickupDirectory)
            .Select(file =>
            {
                using var message = MimeMessage.Load(file);
                return message.ReplyTo.Mailboxes.Single().Address;
            })
            .ToArray();
        Assert.That(
            addresses,
            Is.EquivalentTo(new[] { "first@example.com", "second@example.com" })
        );
    }

    [Test]
    public void ShouldSendLegacyMailDataWithoutReplyToColumn()
    {
        var data = CreateMailData(null);
        data.Mail.Columns.Remove("ReplyTo");

        SendMail(data);

        using var message = MimeMessage.Load(Directory.GetFiles(pickupDirectory).Single());
        Assert.That(message.ReplyTo, Is.Empty);
    }

    [Test]
    public void ShouldRejectInvalidReplyTo()
    {
        Assert.Throws<FormatException>(() => SendMail(CreateMailData("invalid-address")));
        Assert.That(Directory.GetFiles(pickupDirectory), Is.Empty);
    }

    private void SendMail(MailData data)
    {
        var document = new Mock<IDataDocument>();
        document.SetupGet(x => x.DataSet).Returns(data);
        var service = new NetStandardMailService(server: null, port: 25, pickupDirectory);

        Assert.That(
            service.SendMail(document.Object, server: null, port: 25),
            Is.EqualTo(data.Mail.Count)
        );
    }

    private static MailData CreateMailData(string? replyTo)
    {
        var data = new MailData();
        var row = data.Mail.NewMailRow();
        row.Id = Guid.NewGuid();
        row.DateSent = DateTime.UtcNow;
        row.Sender = "sender@example.com";
        row.Recipient = "recipient@example.com";
        row.Subject = "Test";
        row.MessageBody = "Body";
        row.Selected = false;
        if (replyTo != null)
        {
            row.ReplyTo = replyTo;
        }
        data.Mail.AddMailRow(row);
        return data;
    }
}
