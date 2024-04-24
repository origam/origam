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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for EntityFilter.
	/// </summary>
	[SchemaItemDescription("Tree Node", "Nodes", "icon_tree-node.png")]
    [HelpTopic("Tree+Node")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public class TreeStructureNode : AbstractSchemaItem, ISchemaItemFactory, IDataStructureReference
	{
		public const string CategoryConst = "TreeStructure";

		public TreeStructureNode() : base() {Init();}

		public TreeStructureNode(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

		public TreeStructureNode(Key primaryKey) : base(primaryKey)	{Init();}

		private void Init()
		{
			this.ChildItemTypes.Add(typeof(TreeStructureNode));
		}

		#region Overriden AbstractSchemaItem Members
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}
		
		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Properties
		private string _label;

		[NotNullModelElementRule()]
		[XmlAttribute("label")]
        public string Label
		{
			get
			{
				return _label;
			}
			set
			{
				_label = value;
			}
		}

		public Guid NodeIconId;

		[Category("Menu Item")]
		[TypeConverter(typeof(GuiModel.GraphicsConverter))]
        [XmlReference("icon", "NodeIconId")]
		public Graphics NodeIcon
		{
			get
			{
				return (GuiModel.Graphics)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.NodeIconId));
			}
			set
			{
				this.NodeIconId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid DataStructureId;

		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("dataStructure", "DataStructureId")]
        public DataStructure DataStructure
		{
			get
			{
				return (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureId));
			}
			set
			{
				this.Method = null;
				this.SortSet = null;
				this.DataStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid LoadByParentMethodId;

		[TypeConverter(typeof(DataStructureReferenceMethodConverter))]
        [XmlReference("loadByParentMethod", "LoadByParentMethodId")]
        public DataStructureMethod LoadByParentMethod
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.LoadByParentMethodId));
			}
			set
			{
				this.LoadByParentMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		[Browsable(false)]
		public DataStructureMethod Method
		{
			get
			{
				return LoadByParentMethod;
			}
			set
			{
				LoadByParentMethod = value;
			}
		}

		[NotNullModelElementRule()]
		public Guid LoadByPrimaryKeyMethodId;

		[TypeConverter(typeof(DataStructureReferenceMethodConverter))]
        [XmlReference("loadByPrimaryKeyMethod", "LoadByPrimaryKeyMethodId")]
        public DataStructureMethod LoadByPrimaryKeyMethod
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.LoadByPrimaryKeyMethodId));
			}
			set
			{
				this.LoadByPrimaryKeyMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid DataStructureSortSetId;

		[TypeConverter(typeof(DataStructureReferenceSortSetConverter))]
        [XmlReference("sortSet", "DataStructureSortSetId")]
        public DataStructureSortSet SortSet
		{
			get
			{
				return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureSortSetId));
			}
			set
			{
				this.DataStructureSortSetId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		#endregion
	}
}
