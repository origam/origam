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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

public enum PermissionType
{
    Permit = 0,
    Deny = 1,
}

public enum CredentialType
{
    Create = 0,
    Read = 1,
    Update = 2,
    Delete = 3,
}

public enum CredentialValueType
{
    SavedValue = 0,
    ActualValue = 1,
}

/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public abstract class AbstractEntitySecurityRule : AbstractSchemaItem, IComparable
{
    public const string CategoryConst = "EntitySecurityRule";

    public AbstractEntitySecurityRule()
        : base() { }

    public AbstractEntitySecurityRule(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public AbstractEntitySecurityRule(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.Rule != null)
        {
            base.GetParameterReferences(parentItem: Rule, list: list);
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Rule);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null && this.Rule != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Rule.PrimaryKey))
                {
                    this.Rule = item as IEntityRule;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
    {
        return (this.ParentItem.GetType()).Equals(o: newNode.GetType());
    }
    #endregion
    #region Functions
    private void UpdateName()
    {
        string name = Level.ToString() + "_" + this.Type.ToString() + "_" + CredentialsShortcut;
        if (ValueType == CredentialValueType.ActualValue)
        {
            name += "_A";
        }
        if (this.Rule != null)
        {
            name += "_" + this.Rule.Name;
        }
        if (this.Roles != null)
        {
            name += "_" + this.Roles.Replace(oldValue: ";", newValue: "_");
        }

        this.Name = name;
    }
    #endregion
    #region Properties
    internal abstract string CredentialsShortcut { get; }

    internal void CredentialsChanged()
    {
        UpdateName();
    }

    private string _roles = "";

    [Category(category: "Security"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set
        {
            _roles = value;
            UpdateName();
        }
    }
    private PermissionType _permissionType;

    [Category(category: "Security"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "type")]
    public PermissionType Type
    {
        get { return _permissionType; }
        set
        {
            _permissionType = value;
            UpdateName();
        }
    }
    private int _level = 100;

    [
        Category(category: "Security"),
        DefaultValue(value: 100),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "level")]
    public int Level
    {
        get { return _level; }
        set
        {
            _level = value;
            UpdateName();
        }
    }
    private CredentialValueType _valueType = CredentialValueType.SavedValue;

    [
        Category(category: "Security"),
        DefaultValue(value: CredentialValueType.SavedValue),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [NotNullModelElementRule()]
    [XmlAttribute(attributeName: "valueType")]
    public CredentialValueType ValueType
    {
        get { return _valueType; }
        set
        {
            _valueType = value;
            UpdateName();
        }
    }

    public Guid RuleId;

    [TypeConverter(type: typeof(EntityRuleConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Category(category: "Security")]
    [XmlReference(attributeName: "rule", idField: "RuleId")]
    public virtual IEntityRule Rule
    {
        get
        {
            return (IEntityRule)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.RuleId)
                );
        }
        set
        {
            this.RuleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);

            UpdateName();
        }
    }
    #endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        AbstractEntitySecurityRule compared = obj as AbstractEntitySecurityRule;
        if (compared != null)
        {
            // sort first by ValueType
            int tempResult = this.ValueType.CompareTo(target: compared.ValueType);
            if (tempResult == 0) // same
            {
                // then by level
                return this.Level.CompareTo(value: compared.Level);
            }

            return 0 - tempResult; // we reverse because actualValue is more important than savedValue
        }

        return base.CompareTo(obj: obj);
    }
    #endregion
}
