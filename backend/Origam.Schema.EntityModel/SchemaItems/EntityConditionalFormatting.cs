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
using System.Drawing;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[SchemaItemDescription(
    name: "Conditional Formatting Rule",
    folderName: "Conditional Formatting",
    iconName: "icon_conditional-formatting-rule.png"
)]
[HelpTopic(topic: "Conditional+Formatting+Rules")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class EntityConditionalFormatting : AbstractSchemaItem, IComparable
{
    public const string CategoryConst = "EntityConditionalFormatting";

    public EntityConditionalFormatting()
        : base() { }

    public EntityConditionalFormatting(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public EntityConditionalFormatting(Key primaryKey)
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
        if (this.ForegroundColorLookup != null)
        {
            dependencies.Add(item: this.ForegroundColorLookup);
        }

        if (this.BackgroundColorLookup != null)
        {
            dependencies.Add(item: this.BackgroundColorLookup);
        }

        if (this.DynamicColorLookupField != null)
        {
            dependencies.Add(item: this.DynamicColorLookupField);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
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
    #endregion
    #region Functions
    private void UpdateName()
    {
        string name = Level.ToString() + "_" + this.BackgroundColor.ToString();
        if (this.Rule != null)
        {
            name += "_" + this.Rule.Name;
        }
        name += "_" + this.Roles.Replace(oldValue: ";", newValue: "_");
        this.Name = name;
    }
    #endregion
    #region Properties
    internal void CredentialsChanged()
    {
        UpdateName();
    }

    private string _roles = "";

    [Category(category: "Condition"), RefreshProperties(refresh: RefreshProperties.Repaint)]
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

    [Browsable(browsable: false)]
    [XmlAttribute(attributeName: "backgroundColor")]
    public int _backColorInt;

    [Category(category: "Formatting"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    public Color BackgroundColor
    {
        get { return Color.FromArgb(argb: _backColorInt); }
        set
        {
            _backColorInt = value.ToArgb();
            UpdateName();
        }
    }

    [Browsable(browsable: false)]
    [XmlAttribute(attributeName: "foregroundColor")]
    public int _foreColorInt;

    [Category(category: "Formatting"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    public Color ForegroundColor
    {
        get { return Color.FromArgb(argb: _foreColorInt); }
        set
        {
            _foreColorInt = value.ToArgb();
            UpdateName();
        }
    }
    private int _level = 100;

    [
        Category(category: "Condition"),
        DefaultValue(value: 100),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
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
    public Guid RuleId;

    [TypeConverter(type: typeof(EntityRuleConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Category(category: "Condition")]
    [XmlReference(attributeName: "rule", idField: "RuleId")]
    public IEntityRule Rule
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

    public Guid ForeColorLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [Category(category: "Dynamic Color")]
    [XmlReference(attributeName: "foregroundColorLookup", idField: "ForeColorLookupId")]
    public IDataLookup ForegroundColorLookup
    {
        get
        {
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.ForeColorLookupId)
                );
        }
        set
        {
            this.ForeColorLookupId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }

    public Guid BackColorLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [Category(category: "Dynamic Color")]
    [XmlReference(attributeName: "backgroundColorLookup", idField: "BackColorLookupId")]
    public IDataLookup BackgroundColorLookup
    {
        get
        {
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.BackColorLookupId)
                );
        }
        set
        {
            this.BackColorLookupId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public Guid DynamicColorLookupFieldId;

    [TypeConverter(type: typeof(EntityColumnReferenceConverter))]
    [NotNullModelElementRule(
        conditionField1: "ForegroundColorLookup",
        conditionField2: "BackgroundColorLookup"
    )]
    [Category(category: "Dynamic Color")]
    [XmlReference(attributeName: "dynamicColorLookupField", idField: "DynamicColorLookupFieldId")]
    public IDataEntityColumn DynamicColorLookupField
    {
        get
        {
            return (IDataEntityColumn)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DynamicColorLookupFieldId)
                );
        }
        set
        {
            this.DynamicColorLookupFieldId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    #endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        EntityConditionalFormatting compared = obj as EntityConditionalFormatting;
        if (compared != null)
        {
            return this.Level.CompareTo(value: compared.Level);
        }

        return base.CompareTo(obj: obj);
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
        get { return _foreColor; }
        set { _foreColor = value; }
    }
    private Color _backColor;
    public Color BackColor
    {
        get { return _backColor; }
        set { _backColor = value; }
    }
    public bool UseDefaultForeColor
    {
        get
        {
            return this.ForeColor.Equals(obj: Color.FromArgb(alpha: 0, red: 0, green: 0, blue: 0));
        }
    }
    public bool UseDefaultBackColor
    {
        get
        {
            return this.BackColor.Equals(obj: Color.FromArgb(alpha: 0, red: 0, green: 0, blue: 0));
        }
    }
}
