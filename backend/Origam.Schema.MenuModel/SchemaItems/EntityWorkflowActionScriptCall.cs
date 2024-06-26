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
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.MenuModel;
[SchemaItemDescription("Client Script Invocation", "Scripts", 
    "icon_client-script-invocation.png")]
[HelpTopic("Client+Script")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class EntityWorkflowActionScriptCall : AbstractSchemaItem
{
	public const string CategoryConst = "EntityWorkflowActionScriptCall";
	public EntityWorkflowActionScriptCall() : base() {}
	public EntityWorkflowActionScriptCall(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public EntityWorkflowActionScriptCall(Key primaryKey) : base(primaryKey)	{}
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
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
		}
	}
	private string _features;
	[Category("Condition")]
	[XmlAttribute("features")]
    public string Features
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
	public Guid RuleId;
	[Category("Condition")]
	[TypeConverter(typeof(EntityRuleConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("rule", "RuleId")]
	public IEntityRule Rule
	{
		get
		{
            return (IEntityRule)PersistenceProvider.RetrieveInstance(
				typeof(AbstractSchemaItem), new ModelElementKey(RuleId));
		}
		set
		{
			RuleId = (value == null) 
				? Guid.Empty : (Guid)value.PrimaryKey["Id"];
		}
	}
	private int _order = 0;
	[Category("Script")]
	[XmlAttribute("order")]
	public int Order
	{
		get
		{
			return _order;
		}
		set
		{
			_order = value;
		}
	}
	private string _script;
	[Category("Script")]
	[XmlAttribute("script")]
	public string Script
	{
		get { return _script; }
		set { _script = value; }
	}
	public override void GetExtraDependencies(ArrayList dependencies)
	{
		if(Rule != null) dependencies.Add(Rule);
		base.GetExtraDependencies (dependencies);
	}
}
