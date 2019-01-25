#region license
/*
Copyright 2005 - 2017 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Xml;
using System.Data;

using Origam.Schema.WorkflowModel;
using Origam.ServerCommon;
using core = Origam.Workbench.Services.CoreServices;
using Origam.Workbench.Services;

namespace Origam.Server
{
    class WorkQueueSessionStore : SessionStore
    {
        public WorkQueueSessionStore(IBasicUIService service, UIRequest request, string name,
            Analytics analytics)
            : base(service, request, name, analytics)
        {
            IWorkQueueService wqs = ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
            Guid workQueueId = new Guid(request.ObjectId);
            this.WQClass = wqs.WQClass(workQueueId) as WorkQueueClass;
            this.SortSet = this.WQClass.WorkQueueStructureSortSet;
            this.RefreshOnInitUI = true;
        }

        #region Overriden SessionStore Methods

        public override bool HasChanges()
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            DataSet data = LoadWorkQueueData(this.WQClass, new Guid(this.Request.ObjectId));
            SetDataSource(data);
        }

        public override object ExecuteAction(string actionId)
        {
            switch (actionId)
            {
                case ACTION_REFRESH:
                    return Refresh();

                default:
                    throw new ArgumentOutOfRangeException("actionId", actionId, Resources.ErrorContextUnknownAction);
            }
        }

        public override XmlDocument GetFormXml()
        {
            XmlDocument formXml = Origam.OrigamEngine.ModelXmlBuilders.FormXmlBuilder.GetXml(this.WQClass, this.Data, this.Request.Caption, new Guid(this.Request.ObjectId));

            return formXml;
        } 

        private object Refresh()
        {
            Init();
            return this.Data;
        }
        #endregion

        private WorkQueueClass _wqClass;

        public WorkQueueClass WQClass
        {
            get { return _wqClass; }
            set { _wqClass = value; }
        }

        private DataSet LoadWorkQueueData(WorkQueueClass wqc, Guid queueId)
        {
            IWorkQueueService wqs = ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
            return wqs.LoadWorkQueueData(wqc.Name, queueId);
        }
    }
}
