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
using Origam.Schema.EntityModel.Interfaces;

namespace Origam.Schema.GuiModel;

[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public abstract class AbstractPage : AbstractSchemaItem, IAuthorizationContextContainer
{
    public const string CategoryConst = "Page";

    public AbstractPage()
        : base()
    {
        Init();
    }

    public AbstractPage(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public AbstractPage(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.InputValidationRule != null)
        {
            dependencies.Add(item: this.InputValidationRule);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    private void Init() { }

    #region Properties
    private string _url = "";

    [Category(category: "Page")]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "url")]
    public string Url
    {
        get { return _url; }
        set { _url = value; }
    }
    private string _roles;

    [Category(category: "Security")]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }
    public Guid InputValidationRuleId;

    [Category(category: "InputValidation")]
    [Description(
        description: "Validate input parameters. Can validate input parameters before any further action is taken."
    )]
    [TypeConverter(type: typeof(EndRuleConverter))]
    [XmlReference(attributeName: "inputValidationRule", idField: "InputValidationRuleId")]
    public IEndRule InputValidationRule
    {
        get
        {
            return (IEndRule)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.InputValidationRuleId)
                );
        }
        set
        {
            this.InputValidationRuleId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
        }
    }
    private string _features;

    [XmlAttribute(attributeName: "features")]
    public string Features
    {
        get { return _features; }
        set { _features = value; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public Guid CacheMaxAgeDataConstantId;

    [Category(category: "Caching")]
    [Description(
        description: "Sets the number of seconds by which the result should be cached in the user's browser. If not specified the content will not get cached."
    )]
    [TypeConverter(type: typeof(DataConstantConverter))]
    [XmlReference(attributeName: "cacheMaxAge", idField: "CacheMaxAgeDataConstantId")]
    public DataConstant CacheMaxAge
    {
        get
        {
            return (DataConstant)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: CacheMaxAgeDataConstantId)
                );
        }
        set
        {
            CacheMaxAgeDataConstantId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
        }
    }
    private string _mimeType;

    [Category(category: "Page")]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "mimeType")]
    public string MimeType
    {
        get { return _mimeType; }
        set { _mimeType = value; }
    }
    private bool _allowPUT;

    [Category(category: "Updating")]
    [XmlAttribute(attributeName: "allowPut")]
    public bool AllowPUT
    {
        get { return _allowPUT; }
        set { _allowPUT = value; }
    }
    private bool _allowDELETE;

    [Category(category: "Updating")]
    [XmlAttribute(attributeName: "allowDelete")]
    public bool AllowDELETE
    {
        get { return _allowDELETE; }
        set { _allowDELETE = value; }
    }
    #endregion
    #region IAuthorizationContextContainer
    [Browsable(browsable: false)]
    public string AuthorizationContext
    {
        get { return this.Roles; }
    }
    #endregion
}
