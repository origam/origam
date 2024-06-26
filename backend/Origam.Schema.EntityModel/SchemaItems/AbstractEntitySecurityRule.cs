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
using System.Collections.Generic;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using Origam.DA.Common;

namespace Origam.Schema.EntityModel;
public enum PermissionType
{
	Permit = 0,
	Deny = 1
}
public enum CredentialType
{
	Create = 0,
	Read = 1,
	Update = 2,
	Delete = 3
}
public enum CredentialValueType
{
	SavedValue = 0,
	ActualValue = 1
}
/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public abstract class AbstractEntitySecurityRule : AbstractSchemaItem, IComparable
{
	public const string CategoryConst = "EntitySecurityRule";
	public AbstractEntitySecurityRule() : base() {}
	public AbstractEntitySecurityRule(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public AbstractEntitySecurityRule(Key primaryKey) : base(primaryKey)	{}

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
		if(this.Rule != null)
			base.GetParameterReferences(this.Rule as AbstractSchemaItem, list);
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.Rule);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null && this.Rule != null)
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
	public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
	{
		return (this.ParentItem.GetType()).Equals(newNode.GetType());
	}
	#endregion
	#region Functions
	private void UpdateName()
	{
		string name = Level.ToString() + "_" + this.Type.ToString() + "_" + CredentialsShortcut;
		if(ValueType == CredentialValueType.ActualValue)
		{
			name += "_A";
		}
		if(this.Rule != null)
		{
			name += "_" + this.Rule.Name;
		}
		if (this.Roles != null)
		{
			name += "_" + this.Roles.Replace(";", "_");
		}
		
		this.Name = name;
	}
	#endregion
	#region Properties
	internal abstract string CredentialsShortcut{get;}
	internal void CredentialsChanged()
	{
		UpdateName();
	}
	private string _roles = "";
	[Category("Security"), RefreshProperties(RefreshProperties.Repaint)]
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
	private PermissionType _permissionType;
	[Category("Security"), RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
	[XmlAttribute("type")]
    public PermissionType Type
	{
		get
		{
			return _permissionType;
		}
		set
		{
			_permissionType = value;
			UpdateName();
		}
	}
	private int _level = 100;
	[Category("Security"), DefaultValue(100), RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
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
	private CredentialValueType _valueType = CredentialValueType.SavedValue;
	[Category("Security"), DefaultValue(CredentialValueType.SavedValue), RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
    [XmlAttribute("valueType")]
    public CredentialValueType ValueType
	{
		get
		{
			return _valueType;
		}
		set
		{
			_valueType = value;
			UpdateName();
		}
	}
    
	public Guid RuleId;
	[TypeConverter(typeof(EntityRuleConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Category("Security")]
    [XmlReference("rule", "RuleId")]
	public virtual IEntityRule Rule
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
	#endregion
	#region IComparable Members
	public override int CompareTo(object obj)
	{
		AbstractEntitySecurityRule compared = obj as AbstractEntitySecurityRule;
		if(compared != null)
		{
			// sort first by ValueType
			int tempResult = this.ValueType.CompareTo(compared.ValueType);
			if(tempResult == 0) // same
			{
				// then by level
				return this.Level.CompareTo(compared.Level);
			}
			else
			{
				return 0-tempResult; // we reverse because actualValue is more important than savedValue
			}
		}
		else
		{
			return base.CompareTo(obj);
		}
	}
	#endregion
}
