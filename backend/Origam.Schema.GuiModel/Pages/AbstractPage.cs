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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;


namespace Origam.Schema.GuiModel;
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public abstract class AbstractPage : AbstractSchemaItem, IAuthorizationContextContainer
{
	public const string CategoryConst = "Page";
	public AbstractPage() : base() {Init();}
	public AbstractPage(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public AbstractPage(Key primaryKey) : base(primaryKey) {Init();}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{		
		if (this.InputValidationRule != null) dependencies.Add(this.InputValidationRule);			
		base.GetExtraDependencies(dependencies); 
	}
	private void Init()
	{
	}
	#region Properties
	private string _url = "";
	[Category("Page")]
	[StringNotEmptyModelElementRule()]
	[XmlAttribute("url")]
	public string Url
	{
		get
		{
			return _url;
		}
		set
		{
			_url = value;
		}
	}
	private string _roles;
	[Category("Security")]
	[NotNullModelElementRule()]
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
	public Guid InputValidationRuleId;
	[Category("InputValidation")]
	[Description("Validate input parameters. Can validate input parameters before any further action is taken.")]
	[TypeConverter(typeof(EndRuleConverter))]
    [XmlReference("inputValidationRule", "InputValidationRuleId")]
	public IEndRule InputValidationRule
	{
		get
		{
			return (IEndRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.InputValidationRuleId));
		}
		set
		{
			this.InputValidationRuleId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
		}
	}
	private string _features;
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
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
    public Guid CacheMaxAgeDataConstantId;
    [Category("Caching")]
    [Description("Sets the number of seconds by which the result should be cached in the user's browser. If not specified the content will not get cached.")]
    [TypeConverter(typeof(DataConstantConverter))]
    [XmlReference("cacheMaxAge", "CacheMaxAgeDataConstantId")]
    public DataConstant CacheMaxAge
    {
        get
        {
            return (DataConstant)PersistenceProvider.RetrieveInstance(
                typeof(AbstractSchemaItem), new ModelElementKey(CacheMaxAgeDataConstantId));
        }
        set
        {
            CacheMaxAgeDataConstantId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
        }
    }
    private string _mimeType;
    [Category("Page")]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute("mimeType")]
    public string MimeType
    {
        get
        {
            return _mimeType;
        }
        set
        {
            _mimeType = value;
        }
    }
    private bool _allowPUT;
    [Category("Updating")]
    [XmlAttribute("allowPut")]
    public bool AllowPUT
    {
        get
        {
            return _allowPUT;
        }
        set
        {
            _allowPUT = value;
        }
    }
    private bool _allowDELETE;
    [Category("Updating")]
    [XmlAttribute("allowDelete")]
    public bool AllowDELETE
    {
        get
        {
            return _allowDELETE;
        }
        set
        {
            _allowDELETE = value;
        }
    }
    #endregion
    #region IAuthorizationContextContainer
    [Browsable(false)]
	public string AuthorizationContext
	{
		get
		{
			return this.Roles;
		}
	}
	#endregion
}
