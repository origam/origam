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
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for EntitySecurityRule.
	/// </summary>
	[SchemaItemDescription("Row Level Security Rule", "Row Level Security", 
        "icon_row-level-security-rule.png")]
    [HelpTopic("Row+Level+Security+Rules")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class EntitySecurityRule : AbstractEntitySecurityRule
	{
		public EntitySecurityRule() : base() {}

		public EntitySecurityRule(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public EntitySecurityRule(Key primaryKey) : base(primaryKey)	{}
	
		#region Properties
		private bool _create = true;
		[Category("Credentials"), DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
		[Description("If set to true, the rule is applied to create operation.")]
		[XmlAttribute("createCredential")]
        public bool CreateCredential
		{
			get
			{
				return _create;
			}
			set
			{
				_create = value;

				this.CredentialsChanged();
			}
		}

		private bool _update = true;
		[Category("Credentials"), DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
		[Description("If set to true, the rule is applied to update operation.")]
		[XmlAttribute("updateCredential")]
        public bool UpdateCredential
		{
			get
			{
				return _update;
			}
			set
			{
				_update = value;

				this.CredentialsChanged();
			}
		}

		private bool _delete = true;
		[Category("Credentials"), DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
		[Description("If set to true, the rule is applied to delete operation.")]
		[XmlAttribute("deleteCredential")]
        public bool DeleteCredential
		{
			get
			{
				return _delete;
			}
			set
			{
				_delete = value;

				this.CredentialsChanged();
			}
		}

		internal override string CredentialsShortcut
		{
			get
			{
				string result = "";
				result += (this.CreateCredential ? "Create" : "");
				result += (this.UpdateCredential ? "Update" : "");
				result += (this.DeleteCredential ? "Delete" : "");

				return result;
			}
		}

		#endregion
	}
}
