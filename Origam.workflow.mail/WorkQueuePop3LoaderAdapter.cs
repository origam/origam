using System;
using System.Xml;
using Origam.Mail;
using Origam.Workbench.Services;
using Origam.Workflow.WorkQueue;
using NandoF.Mail.PopClient;

namespace Origam.workflow.mail
{
	public class WorkQueuePop3LoaderAdapter : WorkQueueLoaderAdapter
	{
		string _transactionId;
		PopClient _popClient;

		public override void Connect(IWorkQueueService service, Guid queueId, string workQueueClass, string connection, string userName, string password, string transactionId)
		{
			_transactionId = transactionId;

			string server = null;
			int port = 0;
			bool ssl = false;

			string[] cnParts = connection.Split(";".ToCharArray());

			foreach (string part in cnParts)
			{
				string[] pair = part.Split("=".ToCharArray());
				if (pair.Length == 2)
				{
					switch (pair[0].Trim())
					{
						case "server":
							server = pair[1];
							break;
						case "port":
							port = int.Parse(pair[1]);
							break;
						case "ssl":
							ssl = bool.Parse(pair[1]);
							break;
						default:
							throw new ArgumentOutOfRangeException(
                                "connectionParameterName", pair[0], 
                                ResourceUtils.GetString(
                                "ErrorInvalidConnectionString"));
					}
				}
			}

			if(server == null) throw new Exception(
                ResourceUtils.GetString("ErrorNoServer"));
			if(port == 0) throw new Exception(
                ResourceUtils.GetString("ErrorNoString"));

			_popClient = AbstractMailService.GetPopClient(server, port, userName, password, transactionId);

			if(ssl)
			{
				_popClient.IsUsingSsl = true;
			}
		}

		public override void Disconnect()
		{
			_popClient = null;
		}

		public override WorkQueueAdapterResult GetItem(string lastState)
		{
			MailData mailData = new MailData();
			mailData.DataSetName = "ROOT";

			int messageCount = _popClient.GetMessageCount();
			if(messageCount == 0) return null;

			AbstractMailService.RetrieveMail(mailData, _popClient, 1, true);

			WorkQueueAdapterResult result = new WorkQueueAdapterResult(DataDocumentFactory.New(mailData));

            result.Attachments = new WorkQueueAttachment[mailData.MailAttachment.Rows.Count];
			result.State = mailData.Mail[0].MessageId;

			for(int i = 0; i < mailData.MailAttachment.Rows.Count; i++)
			{
                WorkQueueAttachment att = new WorkQueueAttachment();
				att.Data = mailData.MailAttachment[i].Data;
				att.Name = mailData.MailAttachment[i].FileName;

				result.Attachments[i] = att;
			}

            // remove the attachments from the original xml file
            // mailData.MailAttachment.Clear();

			return result;
		}
	}
}
