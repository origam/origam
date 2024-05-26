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
using System.Text;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[SchemaItemDescription("Row Level Security Rule", "Row Level Security", 
	"icon_row-level-security-rule.png")]
[HelpTopic("Row+Level+Security+Rules")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.1.0")]
public class EntitySecurityRule : AbstractEntitySecurityRule
{
	public EntitySecurityRule() : base() {}

	public EntitySecurityRule(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public EntitySecurityRule(Key primaryKey) : base(primaryKey) {}
	
	#region Properties
	private bool create = true;
	[Category("Credentials"), DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
	[Description("If set to true, the rule is applied to create operation.")]
	[XmlAttribute("createCredential")]
	public bool CreateCredential
	{
		get => create;
		set
		{
			create = value;
			CredentialsChanged();
		}
	}

	private bool update = true;
	[Category("Credentials"), DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
	[Description("If set to true, the rule is applied to update operation.")]
	[XmlAttribute("updateCredential")]
	public bool UpdateCredential
	{
		get => update;
		set
		{
			update = value;
			CredentialsChanged();
		}
	}

	private bool delete = true;
	[Category("Credentials"), DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
	[Description("If set to true, the rule is applied to delete operation.")]
	[XmlAttribute("deleteCredential")]
	public bool DeleteCredential
	{
		get => delete;
		set
		{
			delete = value;
			CredentialsChanged();
		}
	}
	
	private bool export = true;
	[Category("Credentials"), DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
	[Description("If set to true, the rule is applied to export operation.")]
	[XmlAttribute("exportCredential")]
	public bool ExportCredential
	{
		get => export;
		set
		{
			export = value;
			CredentialsChanged();
		}
	}

	internal override string CredentialsShortcut
	{
		get
		{
			var stringBuilder = new StringBuilder();
			if (CreateCredential)
			{
				stringBuilder.Append("Create");
			}
			if (UpdateCredential)
			{
				stringBuilder.Append("Update");
			}
			if (DeleteCredential)
			{
				stringBuilder.Append("Delete");
			}
			if (ExportCredential)
			{
				stringBuilder.Append("Export");
			}
			return stringBuilder.ToString();
		}
	}

	#endregion
}