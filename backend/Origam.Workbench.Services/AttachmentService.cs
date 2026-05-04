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
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Workbench.Services;

/// <summary>
/// Summary description for AttachmentService.
/// </summary>
public class AttachmentService : IWorkbenchService, IAttachmentService
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    public AttachmentService() { }

    #region IWorkbenchService Members
    public void UnloadService() { }

    public void InitializeService() { }

    public void AddAttachment(
        string fileName,
        byte[] attachment,
        Guid recordId,
        Guid profileId,
        string transactionId
    )
    {
        DatasetGenerator dsg = new DatasetGenerator(userDefinedParameters: true);
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        DataStructure ds = (DataStructure)
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(ISchemaItem),
                primaryKey: new ModelElementKey(
                    id: new Guid(g: "04a07967-4b59-4c14-8320-e6d073f6f77f")
                )
            );
        DataSet data = dsg.CreateDataSet(ds: ds);
        DataRow r = data.Tables[name: "Attachment"].NewRow();
        r[columnName: "Id"] = Guid.NewGuid();
        r[columnName: "Data"] = attachment;
        r[columnName: "FileName"] = fileName;
        r[columnName: "RecordCreated"] = DateTime.Now;
        r[columnName: "RecordCreatedBy"] = profileId;
        r[columnName: "refParentRecordId"] = recordId;
        data.Tables[name: "Attachment"].Rows.Add(row: r);
        core.DataService.Instance.StoreData(
            dataStructureId: new Guid(g: "04a07967-4b59-4c14-8320-e6d073f6f77f"),
            data: data,
            loadActualValuesAfterUpdate: false,
            transactionId: transactionId
        );
    }

    public void RemoveAttachment(Guid recordId, string transactionId)
    {
        DatasetGenerator dsg = new DatasetGenerator(userDefinedParameters: true);
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        // fetch Attachment by refParentRecordId
        DataSet attachmentDataSet = core.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "04a07967-4b59-4c14-8320-e6d073f6f77f"),
            methodId: new Guid(g: "b3624c91-526d-4b2b-a282-6d99e62a1eb5"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: transactionId,
            paramName1: "Attachment_parRefParentRecordId",
            paramValue1: recordId
        );
        if (attachmentDataSet.Tables[index: 0].Rows.Count == 0)
        {
            // nothing to delete
            return;
        }
        // delete the record
        attachmentDataSet.Tables[index: 0].Rows[index: 0].Delete();
        core.DataService.Instance.StoreData(
            dataStructureId: new Guid(g: "04a07967-4b59-4c14-8320-e6d073f6f77f"),
            data: attachmentDataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: transactionId
        );
        if (log.IsDebugEnabled)
        {
            log.Debug(
                message: string.Format(
                    format: "Attachment with refParentRecordId `{0}' has been successfully removed.",
                    arg0: recordId
                )
            );
        }
    }
    #endregion
}
