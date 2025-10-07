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

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for FormControlSet.
/// </summary>
[SchemaItemDescription("Screen", "icon_screen.png")]
[HelpTopic("Screens")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class FormControlSet : AbstractControlSet
{
    public const string CategoryConst = "FormControlSet";

    public FormControlSet()
        : base() { }

    public FormControlSet(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public FormControlSet(Key primaryKey)
        : base(primaryKey) { }

    //refDataSource means for PanelCOntolSet reference on DataEntity object
    // (for FormControlSet refDataSource means reference on DataStructure object
    [XmlReference("dataStructure", "DataSourceId")]
    public DataStructure DataStructure
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataSourceId;
            return (DataStructure)
                this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
        }
        set { this.DataSourceId = (Guid)value.PrimaryKey["Id"]; }
    }
    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return FormControlSet.CategoryConst; }
    }

    public override Origam.UI.BrowserNodeCollection ChildNodes()
    {
        if (this.ChildItems.Count == 1)
        {
            return new UI.BrowserNodeCollection();
        }
        else
        {
            return base.Alternatives;
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(this.DataStructure);
        base.GetExtraDependencies(dependencies);
    }
    #endregion
}
