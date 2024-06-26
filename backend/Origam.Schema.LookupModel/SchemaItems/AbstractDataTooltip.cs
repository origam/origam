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

using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using System.Xml.Serialization;

namespace Origam.Schema.LookupModel;
/// <summary>
/// Summary description for AbstractDataTooltip.
/// </summary>
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class AbstractDataTooltip : AbstractSchemaItem, IComparable
{
	public const string CategoryConst = "DataTooltip";
	public AbstractDataTooltip() : base() {}
	public AbstractDataTooltip(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public AbstractDataTooltip(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
    public override void GetParameterReferences(AbstractSchemaItem parentItem, System.Collections.Hashtable list)
    {
        base.GetParameterReferences(this.TooltipLoadMethod, list);
    }
    
    public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.TooltipDataStructure);
        dependencies.Add(this.TooltipLoadMethod);
        dependencies.Add(this.TooltipTransformation);
		base.GetExtraDependencies (dependencies);
	}
	public override SchemaItemCollection ChildItems
	{
		get
		{
			return new SchemaItemCollection();
		}
	}
	public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
	{
		return newNode is AbstractDataLookup;
	}
	#endregion
	#region Properties
	public Guid TooltipDataStructureId;
	[Category("Tooltip")]
	[TypeConverter(typeof(DataStructureConverter))]
	[NotNullModelElementRule()]
    [XmlReference("looltipDataStructure", "TooltipDataStructureId")]
    public DataStructure TooltipDataStructure
	{
		get
		{
			return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.TooltipDataStructureId)) as DataStructure;
		}
		set
		{
			this.TooltipDataStructureId = (Guid)value.PrimaryKey["Id"];
            this.TooltipLoadMethod = null;
		}
	}
    
    public Guid TooltipDataStructureMethodId;
    [TypeConverter(typeof(DataServiceDataTooltipFilterConverter))]
    [Category("Tooltip")]
    [NotNullModelElementRule("TooltipDataStructure")]
    [XmlReference("tooltipLoadMethod", "TooltipDataStructureMethodId")]
    public DataStructureMethod TooltipLoadMethod
    {
        get
        {
            return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.TooltipDataStructureMethodId));
        }
        set
        {
            this.TooltipDataStructureMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
        }
    }
	public Guid TooltipTransformationId;
	[Category("Tooltip")]
	[TypeConverter(typeof(TransformationConverter))]
	[NotNullModelElementRule()]
    [XmlReference("tooltipTransformation", "TooltipTransformationId")]
    public ITransformation TooltipTransformation
	{
		get
		{
			return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.TooltipTransformationId)) as ITransformation;
		}
		set
		{
			this.TooltipTransformationId = (Guid)value.PrimaryKey["Id"];
		}
	}
	private string _roles = "*";
	[Category("Condition"), DefaultValue("*")]
	[XmlAttribute("roles")]
	public virtual string Roles
	{
		get
		{
			if(_roles == null)
			{
				return "*";
			}
			else
			{
				return _roles;
			}
		}
		set
		{
			_roles = value;
		}
	}
	private string _features;
	[Category("Condition")]
	[XmlAttribute("features")]
    public virtual string Features
	{
		get
		{
			return _features;
		}
		set
		{
			_features = value;
		}
	}
	private int _level = 100;
	[Category("Condition"), DefaultValue(100), RefreshProperties(RefreshProperties.Repaint)]
	[XmlAttribute("level")]
    public int Level
	{
		get
		{
			return _level;
		}
		set
		{
			_level = value;
		}
	}
	#endregion
	#region IComparable Members
	public override int CompareTo(object obj)
	{
		EntityFieldDynamicLabel compared = obj as EntityFieldDynamicLabel;
		if(compared != null)
		{
			return this.Level.CompareTo(compared.Level);
		}
		else
		{
			return base.CompareTo(obj);
		}
	}
	#endregion
}
