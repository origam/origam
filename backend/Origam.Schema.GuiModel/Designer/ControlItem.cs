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

namespace Origam.Schema.GuiModel;

public enum ControlToolBoxVisibility
{
    Nowhere,
    PanelDesigner,
    FormDesigner,
    PanelAndFormDesigner,
};

/// <summary>
/// Summary description for ControlItem.
/// </summary>
[SchemaItemDescription(name: "Widget", iconName: "icon_widget.png")]
[HelpTopic(topic: "Widgets")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ControlItem : AbstractSchemaItem, ISchemaItemFactory
{
    public const string CategoryConst = "Control";

    public ControlItem()
        : base()
    {
        Init();
    }

    public ControlItem(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public ControlItem(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.Add(item: typeof(ControlPropertyItem));
        this.ChildItemTypes.Add(item: typeof(ControlStyleProperty));
    }

    #region Properties
    private ControlToolBoxVisibility _controlToolBoxVisibility;

    [XmlAttribute(attributeName: "toolboxVisibility")]
    public ControlToolBoxVisibility ControlToolBoxVisibility
    {
        get { return _controlToolBoxVisibility; }
        set { _controlToolBoxVisibility = value; }
    }
    private string _controlType;

    [XmlAttribute(attributeName: "typeName")]
    public string ControlType
    {
        get { return _controlType; }
        set { _controlType = value; }
    }
    private string _controlNamespace;

    [XmlAttribute(attributeName: "namespace")]
    public string ControlNamespace
    {
        get { return _controlNamespace; }
        set { _controlNamespace = value; }
    }
    private bool _isComplexType;

    [XmlAttribute(attributeName: "isComplex")]
    public bool IsComplexType
    {
        get { return _isComplexType; }
        set { _isComplexType = value; }
    }
    public Guid PanelControlSetId;

    [XmlReference(attributeName: "screenSection", idField: "PanelControlSetId")]
    public PanelControlSet PanelControlSet
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.PanelControlSetId;

            try
            {
                return (PanelControlSet)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(PanelControlSet),
                        primaryKey: key
                    );
            }
            catch
            {
                return null;
            }
        }
        set
        {
            if (value != null)
            {
                this.PanelControlSetId = (Guid)value.PrimaryKey[key: "Id"];
            }
            else
            {
                this.PanelControlSetId = System.Guid.Empty;
            }
        }
    }
    private bool _requestSaveAfterChangeAllowed;

    [XmlAttribute(attributeName: "requestSaveAfterChangeAllowed")]
    public bool RequestSaveAfterChangeAllowed
    {
        get { return _requestSaveAfterChangeAllowed; }
        set { _requestSaveAfterChangeAllowed = value; }
    }
    #endregion
    #region Overriden ISchemaItem Members
    [Browsable(browsable: false)]
    public override bool CanDelete
    {
        get
        {
            if (this.PanelControlSet == null)
            {
                return true;
            }

            return false;
        }
    }
    public override string ItemType
    {
        get { return ControlItem.CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        // we will not add panel control set, because panel handles deletion of this control,
        // instead, ControlSetItem references directly the Panel definition
        //if(this.PanelControlSet != null) dependencies.Add(this.PanelControlSet);
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
}
