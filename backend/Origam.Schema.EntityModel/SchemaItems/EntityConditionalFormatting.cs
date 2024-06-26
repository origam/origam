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
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[SchemaItemDescription("Conditional Formatting Rule", 
    "Conditional Formatting", 
    "icon_conditional-formatting-rule.png")]
[HelpTopic("Conditional+Formatting+Rules")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class EntityConditionalFormatting : AbstractSchemaItem, IComparable
{
	public const string CategoryConst = "EntityConditionalFormatting";
	public EntityConditionalFormatting() : base() {}
	public EntityConditionalFormatting(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public EntityConditionalFormatting(Key primaryKey) : base(primaryKey)	{}

	#region Overriden AbstractDataEntityColumn Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override void GetParameterReferences(AbstractSchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		if(this.Rule != null) base.GetParameterReferences(this.Rule as AbstractSchemaItem, list);
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.Rule);
		if(this.ForegroundColorLookup != null) dependencies.Add(this.ForegroundColorLookup);
		if(this.BackgroundColorLookup != null) dependencies.Add(this.BackgroundColorLookup);
		if(this.DynamicColorLookupField != null) dependencies.Add(this.DynamicColorLookupField);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.Rule.PrimaryKey))
				{
					this.Rule = item as IEntityRule;
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	public override SchemaItemCollection ChildItems
	{
		get
		{
			return new SchemaItemCollection();
		}
	}
	#endregion
	#region Functions
	private void UpdateName()
	{
		string name = Level.ToString() + "_" + this.BackgroundColor.ToString();
		if(this.Rule != null)
		{
			name += "_" + this.Rule.Name;
		}
		name += "_" + this.Roles.Replace(";", "_");
		this.Name = name;
	}
	#endregion
	#region Properties
	internal void CredentialsChanged()
	{
		UpdateName();
	}
	private string _roles = "";
	[Category("Condition"), RefreshProperties(RefreshProperties.Repaint)]
	[StringNotEmptyModelElementRule()]
    [XmlAttribute("roles")]
    public string Roles
	{
		get
		{
			return _roles;
		}
		set
		{
			_roles = value;
			UpdateName();
		}
	}
    
	[Browsable(false)]
    [XmlAttribute("backgroundColor")]
    public int _backColorInt;
	[Category("Formatting"), RefreshProperties(RefreshProperties.Repaint)]
	public Color BackgroundColor
	{
		get
		{
			return Color.FromArgb(_backColorInt);
		}
		set
		{
			_backColorInt = value.ToArgb();
			UpdateName();
		}
	}
	
	[Browsable(false)]
    [XmlAttribute("foregroundColor")]
    public int _foreColorInt;
	[Category("Formatting"), RefreshProperties(RefreshProperties.Repaint)]
	public Color ForegroundColor
	{
		get
		{
			return Color.FromArgb(_foreColorInt);
		}
		set
		{
			_foreColorInt = value.ToArgb();
			UpdateName();
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
			UpdateName();
		}
	}
	public Guid RuleId;
	[TypeConverter(typeof(EntityRuleConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Category("Condition")]
    [XmlReference("rule", "RuleId")]
    public IEntityRule Rule
	{
		get
		{
            return (IEntityRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.RuleId));
		}
		set
		{
			this.RuleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		
			UpdateName();
		}
	}
    
	public Guid ForeColorLookupId;
	[TypeConverter(typeof(DataLookupConverter))]
	[Category("Dynamic Color")]
    [XmlReference("foregroundColorLookup", "ForeColorLookupId")]
    public IDataLookup ForegroundColorLookup
	{
		get
		{
			return (IDataLookup)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ForeColorLookupId));
		}
		set
		{
			this.ForeColorLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
    
	public Guid BackColorLookupId;
	[TypeConverter(typeof(DataLookupConverter))]
	[Category("Dynamic Color")]
    [XmlReference("backgroundColorLookup", "BackColorLookupId")]
    public IDataLookup BackgroundColorLookup
	{
		get
		{
			return (IDataLookup)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.BackColorLookupId));
		}
		set
		{
			this.BackColorLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
	public Guid DynamicColorLookupFieldId;
	[TypeConverter(typeof(EntityColumnReferenceConverter))]
	[NotNullModelElementRule("ForegroundColorLookup", "BackgroundColorLookup")]
	[Category("Dynamic Color")]
    [XmlReference("dynamicColorLookupField", "DynamicColorLookupFieldId")]
    public IDataEntityColumn DynamicColorLookupField
	{
		get
		{
			return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DynamicColorLookupFieldId));
		}
		set
		{
			this.DynamicColorLookupFieldId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
	#endregion
	#region IComparable Members
	public override int CompareTo(object obj)
	{
		EntityConditionalFormatting compared = obj as EntityConditionalFormatting;
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
public class EntityFormatting
{
	public EntityFormatting(Color foreColor, Color backColor)
	{
		_foreColor = foreColor;
		_backColor = backColor;
	}
	private Color _foreColor;
	public Color ForeColor
	{
		get
		{
			return _foreColor;
		}
		set
		{
			_foreColor = value;
		}
	}
	private Color _backColor;
	public Color BackColor
	{
		get
		{
			return _backColor;
		}
		set
		{
			_backColor = value;
		}
	}
	public bool UseDefaultForeColor
	{
		get
		{
			return this.ForeColor.Equals(Color.FromArgb(0,0,0,0));
		}
	}
	public bool UseDefaultBackColor
	{
		get
		{
			return this.BackColor.Equals(Color.FromArgb(0,0,0,0));
		}
	}
}
