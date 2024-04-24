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
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using Origam.DA;
using Origam.DA.Common;
using Origam.Schema.EntityModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.RuleModel
{
    /// <summary>
    /// Summary description for AbstractRule.
    /// </summary>
    [XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public abstract class AbstractRule : AbstractSchemaItem, IRule
	{
		public const string CategoryConst = "Rule";

		public AbstractRule() : base() {}

		public AbstractRule(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public AbstractRule(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			if(this.Structure != null) dependencies.Add(this.Structure);

			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region Properties
		[Category("Rule")]
		[XmlAttribute ("dataType")]
		public OrigamDataType DataType { get; set; }

		public Guid DataStructureId;

		[Category("Rule")]
		[TypeConverter(typeof(DataStructureConverter))]
        [XmlReference("dataStructure", "DataStructureId")]
		public IDataStructure Structure
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.DataStructureId;

				return (IDataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.DataStructureId = Guid.Empty;
				}
				else
				{
					this.DataStructureId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}

		public abstract bool IsPathRelative{get;set;}
		
		[DefaultValue(Trace.InheritFromParent)]
		[Category("Tracing"), RefreshProperties(RefreshProperties.Repaint)]
		[RuntimeConfigurable ("traceLevel")]
		[DisplayName("Trace Level")]
		public Trace TraceLevel { get; set; } = Trace.InheritFromParent;

		[Category("Tracing")]
   
		public Trace Trace => TraceLevel;

		#endregion

	}
}
