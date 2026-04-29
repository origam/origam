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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for RuleReference.
/// </summary>
[SchemaItemDescription(name: "Report Reference", iconName: "icon_report-reference.png")]
[HelpTopic(topic: "Report+Reference")]
[DefaultProperty(name: "Report")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ReportReference : AbstractSchemaItem
{
    public const string CategoryConst = "ReportReference";

    public ReportReference()
        : base() { }

    public ReportReference(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public ReportReference(Key primaryKey)
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
        if (this.Report != null)
        {
            base.GetParameterReferences(parentItem: Report, list: list);
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Report);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    public Guid ReportId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(ReportConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "report", idField: "ReportId")]
    public AbstractReport Report
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ReportId;
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: key
                    ) as AbstractReport;
        }
        set
        {
            this.ReportId = (Guid)value.PrimaryKey[key: "Id"];
            //this.Name = this.Report.Name;
        }
    }
    #endregion
}
