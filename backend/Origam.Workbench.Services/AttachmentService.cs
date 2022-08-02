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

using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.DA;
using Origam.DA.Service;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// Summary description for AttachmentService.
	/// </summary>
	public class AttachmentService : IWorkbenchService, IAttachmentService
	{
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AttachmentService()
		{
		}

		#region IWorkbenchService Members

		public void UnloadService()
		{
		}

		public void InitializeService()
		{
		}

		public void AddAttachment(string fileName, byte[] attachment, Guid recordId, Guid profileId, string transactionId)
		{
			DatasetGenerator dsg = new DatasetGenerator(true);
			IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			DataStructure ds = (DataStructure)ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f")));

			DataSet data = dsg.CreateDataSet(ds);
			DataRow r = data.Tables["Attachment"].NewRow();

			r["Id"] = Guid.NewGuid();
			r["Data"] = attachment;
			r["FileName"] = fileName;
			r["RecordCreated"] = DateTime.Now;
			r["RecordCreatedBy"] = profileId;
			r["refParentRecordId"] = recordId;

			data.Tables["Attachment"].Rows.Add(r);

			core.DataService.Instance.StoreData(new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f"), data, false, transactionId);
		}

        public void RemoveAttachment(Guid recordId, string transactionId)
        {
            DatasetGenerator dsg = new DatasetGenerator(true);
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;

            // fetch Attachment by refParentRecordId
            DataSet attachmentDataSet = core.DataService.Instance.LoadData(new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f"),
                new Guid("b3624c91-526d-4b2b-a282-6d99e62a1eb5"), Guid.Empty, Guid.Empty, transactionId,
                "Attachment_parRefParentRecordId", recordId);
            if (attachmentDataSet.Tables[0].Rows.Count == 0)
            {
                // nothing to delete
                return;
            }
            // delete the record           
            attachmentDataSet.Tables[0].Rows[0].Delete();
            core.DataService.Instance.StoreData(new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f"), attachmentDataSet, false, transactionId);
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Attachment with refParentRecordId `{0}' has been successfully removed.",
                    recordId));
            }
        }
		#endregion
	}
}
