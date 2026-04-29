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

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for ServiceMethod.
/// </summary>
[SchemaItemDescription(name: "Action", folderName: "Actions", icon: 18)]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class AbstractWorkflowPageAction : AbstractSchemaItem
{
    public const string CategoryConst = "WorkflowPageAction";

    public AbstractWorkflowPageAction()
        : base()
    {
        Init();
    }

    public AbstractWorkflowPageAction(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public AbstractWorkflowPageAction(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.Add(item: typeof(WorkflowPageActionParameter));
    }

    #region Properties
    public Guid ConditionRuleId;

    [Category(category: "Conditions")]
    [TypeConverter(type: typeof(StartRuleConverter))]
    [XmlReference(attributeName: "conditionRule", idField: "ConditionRuleId")]
    public IStartRule ConditionRule
    {
        get
        {
            return (IStartRule)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.ConditionRuleId)
                );
        }
        set
        {
            this.ConditionRuleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
        }
    }
    private int _sortOrder = 100;

    [DefaultValue(value: 100)]
    [XmlAttribute(attributeName: "sortOrder")]
    public int SortOrder
    {
        get { return _sortOrder; }
        set { _sortOrder = value; }
    }
    private string _roles = "*";

    [Category(category: "Conditions")]
    [NotNullModelElementRule()]
    [DefaultValue(value: "*")]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }
    private string _features;

    [Category(category: "Conditions")]
    [XmlAttribute(attributeName: "features")]
    public string Features
    {
        get { return _features; }
        set { _features = value; }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override bool UseFolders
    {
        get { return false; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        base.GetExtraDependencies(dependencies: dependencies);
        if (this.ConditionRule != null)
        {
            dependencies.Add(item: this.ConditionRule);
        }
    }
    #endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        AbstractWorkflowPageAction compareItem = obj as AbstractWorkflowPageAction;
        if (compareItem == null)
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorCompareAbstractWorkflowPageAction")
            );
        }

        return this.SortOrder.CompareTo(value: compareItem.SortOrder);
    }
    #endregion
}
