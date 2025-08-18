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
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for EntityFieldConditionalFormatting.
/// </summary>
/// <summary>
/// Summary description for DataEntityIndex.
/// </summary>
[SchemaItemDescription("Dynamic Field Label", "Dynamic Labels", 5)]
[HelpTopic("Dynamic+Field+Labels")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class EntityFieldDynamicLabel : AbstractSchemaItem, IComparable
{
	public EntityFieldDynamicLabel() : base(){}
	
	public EntityFieldDynamicLabel(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public EntityFieldDynamicLabel(Key primaryKey) : base(primaryKey)	{}
	public const string CategoryConst = "DataEntityFieldDynamicLabel";
	#region Properties
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
            return (IEntityRule)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(this.RuleId));
		}
		set
		{
			this.RuleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		
			UpdateName();
		}
	}
    
	public Guid LabelConstantId;
	[TypeConverter(typeof(DataConstantConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
	[Category("Label")]
    [XmlReference("labelConstant", "LabelConstantId")]
    public DataConstant LabelConstant
	{
		get
		{
			return (DataConstant)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(this.LabelConstantId));
		}
		set
		{
			this.LabelConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		
			UpdateName();
		}
	}
	/// <summary>
	/// Returns if there is no rule and roles, so the label is actually static (never changes).
	/// </summary>
	public bool IsStatic
	{
		get
		{
			return this.Rule == null && this.Roles == "*";
		}
	}
	#endregion
	#region Functions
	private void UpdateName()
	{
		string name = Level.ToString();
		if(this.LabelConstant != null)
		{
			name += "_" + this.LabelConstant.Name;
		}
		if(this.Rule != null)
		{
			name += "_" + this.Rule.Name;
		}
		name += "_" + this.Roles.Replace(";", "_");
		this.Name = name;
	}
	#endregion
	#region Overriden ISchemaItem Members
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		if(this.Rule != null) dependencies.Add(this.Rule);
		if(this.LabelConstant != null) dependencies.Add(this.LabelConstant);
		base.GetExtraDependencies (dependencies);
	}
	public override string Icon
	{
		get
		{
			return "5";
		}
	}
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	#endregion
	#region IComparable Members
	public override int CompareTo(object obj)
	{
		EntityFieldDynamicLabel compared = obj as EntityFieldDynamicLabel;
		if(obj != null)
		{
			return this.Level.CompareTo(compared.Level);
		}
		else
		{
			throw new ArgumentOutOfRangeException("obj", obj, ResourceUtils.GetString("ErrorCompareEntityConditionalFormatting"));
		}
	}
	#endregion
}
