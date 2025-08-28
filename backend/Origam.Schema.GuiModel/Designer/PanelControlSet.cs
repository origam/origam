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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for PanelControlSet.
/// </summary>
[SchemaItemDescription("Screen Section", "icon_screen-section.png")]
[System.Drawing.ToolboxBitmap(typeof(PanelControlSet))]
[HelpTopic("Screen+Sections")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class PanelControlSet : AbstractControlSet
{
    private static ISchemaService _schema =
        ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
    private UserControlSchemaItemProvider _controls =
        _schema.GetProvider(typeof(UserControlSchemaItemProvider)) as UserControlSchemaItemProvider;
    public const string CategoryConst = "PanelControlSet";

    public PanelControlSet()
        : base() { }

    public PanelControlSet(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public PanelControlSet(Key primaryKey)
        : base(primaryKey) { }

    //refDataSource means for PanelCOntolSet reference on DataEntity object
    // (for FormControlSet refDataSource means reference on DataStructure object
    [XmlReference("entity", "DataSourceId")]
    public IDataEntity DataEntity
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataSourceId;
            return (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
        }
        set { this.DataSourceId = (Guid)value.PrimaryKey["Id"]; }
    }
    public ControlItem PanelControl
    {
        get
        {
            foreach (ControlItem item in _controls.ChildItems)
            {
                if (
                    item.PanelControlSet != null
                    && item.PanelControlSet.PrimaryKey.Equals(this.PrimaryKey)
                    && item.IsComplexType
                    && (!item.IsDeleted)
                )
                {
                    return item;
                }
            }
            return null;
        }
    }

    #region Overriden ISchemaItem Members
    public override bool IsDeleted
    {
        get { return base.IsDeleted; }
        set
        {
            //1) find controlItem
            ControlItem item = this.PanelControl;
            if (item != null)
            {
                //2) delete reference in ControlItem and set is complex on false
                item.PanelControlSet = null;
                item.IsComplexType = false;
                //3) delete founded ControlItem
                item.IsDeleted = true;
                item.Persist();
            }
            //if all done delete main control
            base.IsDeleted = value;
        }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override Origam.UI.BrowserNodeCollection ChildNodes()
    {
        if (this.ChildItems.Count == 1)
        {
            return new UI.BrowserNodeCollection();
        }

        return base.Alternatives;
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(this.DataEntity);

        base.GetExtraDependencies(dependencies);
    }
    #endregion
}
