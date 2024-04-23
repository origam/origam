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

namespace Origam.Workflow.WorkQueue;

/// <summary>
/// Summary description for WorkQueueAdapterFactory.
/// </summary>
public class WorkQueueAdapterFactory
{
	public static WorkQueueLoaderAdapter GetAdapter(string adapterId)
	{
			switch(adapterId)
			{
				case "57bed127-45cc-46f1-b29b-53c635f665b3":	// IMAP
					return Reflector.InvokeObject("Origam.workflow.mail.WorkQueueImapLoaderAdapter", "Origam.workflow.mail") as WorkQueueLoaderAdapter;
				case "07329b7b-90e8-4594-b738-c04856fc998e":	// FILE
					return new WorkQueueFileLoader();
				case "4c15f1a1-4bd8-4fa6-9a37-df5aa19f02a5":	// POP3
					return Reflector.InvokeObject("Origam.workflow.mail.WorkQueuePop3LoaderAdapter", "Origam.workflow.mail") as WorkQueueLoaderAdapter;
				case "75e3b51a-e4f5-48ed-941c-597f49fcc775":	// Sequential Workflow
					return new WorkQueueWorkflowLoader();
				case "cb882379-80e7-41fd-bde7-c65045660ca7":	// Web Request
					return new WorkQueueWebLoader();
                case "58c4ef05-537b-489e-bdf1-bbc3ed109e55":    // WebSphere MQ
                    return Reflector.InvokeObject("Origam.Workflow.WorkQueue.WorkQueueWebSphereMQLoader", "Origam.Workflow.WorkQueue.WebSphereMQLoader") as WorkQueueLoaderAdapter;
                case "76402268-6e7e-4ee4-bbf5-ae054b2eb793":    // IATA BSP File
					return new WorkQueueIataBspFileLoader();
                case "5c43e42b-d9e4-4a2e-9d3c-d12193acc390":    // indexed file
                    return new WorkQueueIncrementalFileLoader();
                default:
					throw new ArgumentOutOfRangeException("refWorkQueueExternalSourceTypeId", adapterId, ResourceUtils.GetString("ErrorUnknownWorkQueueAdapter"));
			}
		}
}