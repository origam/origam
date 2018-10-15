using System;
using Origam.Workbench.Services;

namespace Origam.Workflow
{
	/// <summary>
	/// Summary description for WorkQueueXmlFileLoaderAdapter.
	/// </summary>
	public class WorkQueueXmlFileLoaderAdapter : WorkQueueLoaderAdapter
	{
		public WorkQueueXmlFileLoaderAdapter()
		{
		}

		public override void Connect(IWorkQueueService service, Guid queueId, string workQueueClass, string connection, string userName, string password)
		{

		}

		public override void Disconnect()
		{

		}

		public override System.Xml.XmlDocument GetItem()
		{
			return null;
		}
	}
}
