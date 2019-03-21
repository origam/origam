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

using System.Collections.Generic;
using System.Data;
using Origam.Schema.GuiModel;

namespace Origam.Server
{
    class TreeNode
    {
        public string Id;
        public string ParentId;
        public string Type;
        public string Label;
        public string IconUrl;
        public bool CanUpdate = true;
        public bool CanDelete = true;
        public IList<string> ChildTypes = new List<string>();

        public TreeNode()
        {
        }

        public TreeNode(DataRow row, TreeStructureNode node)
        {
            this.Id = row["Id"].ToString();
            this.Label = (string)row["Name"];
            if (row.Table.Columns.Contains("ParentId"))
            {
                this.ParentId = row["ParentId"].ToString();
            }
            this.Type = node.Id.ToString();
            this.IconUrl = node.NodeIcon.Name;
            this.CanDelete = true;
            this.CanUpdate = true;

            foreach (TreeStructureNode child in node.ChildItemsByType(TreeStructureNode.ItemTypeConst))
            {
                this.ChildTypes.Add(child.Id.ToString());
            }
        }
    }
}
