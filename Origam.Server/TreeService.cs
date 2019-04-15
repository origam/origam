#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;

using FluorineFx;
using Origam.Workbench.Services;
using Origam.Schema.GuiModel;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.DA;
using Origam.DA.Service;
using System.Data;
using core = Origam.Workbench.Services.CoreServices;
using System.Collections;

namespace Origam.Server
{   
    [RemotingService]
    class TreeService
    {
        public TreeNode RootNode(string treeId)
        {
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            TreeStructure ts = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(treeId))) as TreeStructure;

            TreeNode node = new TreeNode();

            node.Id = treeId;
            node.Type = treeId;
            node.Label = ts.RootNodeLabel;
            node.IconUrl = "menu_folder.png";
            node.CanDelete = false;
            node.CanUpdate = false;

            foreach (TreeStructureNode child in ts.ChildItemsByType(TreeStructureNode.ItemTypeConst))
            {
                node.ChildTypes.Add(child.Id.ToString());
            }

            return node;
        }

        public IList<TreeNode> GetNodes(string treeId, string parentTypeId, string parentId)
        {
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            TreeStructure ts = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(treeId))) as TreeStructure;
            AbstractSchemaItem tn = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(parentTypeId))) as AbstractSchemaItem;

            List<TreeNode> result = new List<TreeNode>();

            foreach (TreeStructureNode childNode in tn.ChildItemsByType(TreeStructureNode.ItemTypeConst))
            {
                QueryParameterCollection parameters = GetParentKeyParameters(childNode, parentId);
                DataSet data = core.DataService.LoadData(childNode.DataStructureId, childNode.LoadByParentMethodId, Guid.Empty, childNode.DataStructureSortSetId, null, parameters);
                foreach (DataRow row in data.Tables["Node"].Rows)
                {
                    TreeNode resultNode = new TreeNode(row, childNode);
                    result.Add(resultNode);
                }
            }

            return result;
        }

        public TreeNode AddNode(string treeId, string parentId, string typeId, string label)
        {
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            TreeStructure ts = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(treeId))) as TreeStructure;
            TreeStructureNode tn = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(typeId))) as TreeStructureNode;

            DatasetGenerator dg = new DatasetGenerator(true);
            DataSet data = dg.CreateDataSet(tn.DataStructure);

            DataTable table = data.Tables["Node"];
            DataRow row = table.NewRow();
            row["Id"] = Guid.NewGuid();
            row["RecordCreated"] = DateTime.Now;
            row["RecordCreatedBy"] = SecurityTools.CurrentUserProfile().Id;
            row["Name"] = label;
            if (row.Table.Columns.Contains("ParentId"))
            {
                row["ParentId"] = parentId;
            }

            table.Rows.Add(row);

            core.DataService.StoreData(tn.DataStructureId, data, false, null);

            TreeNode node = new TreeNode(row, tn);
            return node;
        }

        public TreeNode UpdateNode(string treeId, string parentId, string typeId, string id, string newLabel)
        {
            return UpdateNode(treeId, parentId, typeId, id, newLabel, true, false);
        }

        private TreeNode UpdateNode(string treeId, string parentId, string typeId, string id, string newLabel, bool updateLabel, bool updateParentId)
        {
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            TreeStructure ts = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(treeId))) as TreeStructure;
            TreeStructureNode tn = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(typeId))) as TreeStructureNode;

            DataSet data = LoadNode(id, tn);

            DataRow row = data.Tables["Node"].Rows[0];
            if (updateLabel)
            {
                row["Name"] = newLabel;
            }
            if (updateParentId && row.Table.Columns.Contains("ParentId"))
            {
                row["ParentId"] = parentId;
            }
            row["RecordUpdated"] = DateTime.Now;
            row["RecordUpdatedBy"] = SecurityTools.CurrentUserProfile().Id;

            core.DataService.StoreData(tn.DataStructureId, data, false, null);

            TreeNode node = new TreeNode(row, tn);

            return node;
        }

        private static QueryParameterCollection GetPrimaryKeyParameters(TreeStructureNode tn, string id)
        {
            QueryParameterCollection parameters = new QueryParameterCollection();

            if (tn.LoadByPrimaryKeyMethod != null)
            {
                ICollection keys = tn.LoadByPrimaryKeyMethod.ParameterReferences.Keys;
                foreach (string parameterName in keys)
                {
                    parameters.Add(new QueryParameter(parameterName, id));
                }
            }
            return parameters;
        }

        private static QueryParameterCollection GetParentKeyParameters(TreeStructureNode tn, string id)
        {
            QueryParameterCollection parameters = new QueryParameterCollection();
            if (tn.LoadByParentMethod != null)
            {
                ICollection keys = tn.LoadByParentMethod.ParameterReferences.Keys;
                foreach (string parameterName in keys)
                {
                    parameters.Add(new QueryParameter(parameterName, id));
                }
            }
            return parameters;
        }

        public IList<TreeNode> MoveNode(string treeId, string typeId, string parentTypeId, string id, string newParentId)
        {
            this.UpdateNode(treeId, newParentId, typeId, id, null, false, true);

            return GetNodes(treeId, parentTypeId, newParentId);
        }

        public void DeleteNode(string treeId, string parentId, string typeId, string id)
        {
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            TreeStructure ts = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(treeId))) as TreeStructure;
            TreeStructureNode tn = ps.SchemaProvider.RetrieveInstance(typeof(TreeStructure), new ModelElementKey(new Guid(typeId))) as TreeStructureNode;

            DataSet data = LoadNode(id, tn);

            DataRow row = data.Tables["Node"].Rows[0];
            row.Delete();

            if (GetNodes(treeId, typeId, id).Count > 0)
            {
                throw new Exception(Properties.Resources.ErrorTreeHasChildrenCannotDelete);
            }
            if (core.DataService.ReferenceCount(((DataStructureEntity)tn.DataStructure.Entities[0]).EntityId, id, null) > 0)
            {
                throw new Exception(Properties.Resources.ErrorTreeItemUsedCannotDelete);
            }

            core.DataService.StoreData(tn.DataStructureId, data, false, null);
        }

        private static DataSet LoadNode(string id, TreeStructureNode tn)
        {
            QueryParameterCollection parameters = GetPrimaryKeyParameters(tn, id);

            DataSet data = core.DataService.LoadData(tn.DataStructureId, tn.LoadByPrimaryKeyMethodId, Guid.Empty, tn.DataStructureSortSetId, null, parameters);
            return data;
        }
    }
}
