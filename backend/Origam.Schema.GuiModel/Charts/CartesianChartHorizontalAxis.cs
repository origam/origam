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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription(
    name: "Horizontal Axis",
    folderName: "Horizontal Axes",
    iconName: "icon_horizontal-axis.png"
)]
[HelpTopic(topic: "Horizontal+Axis")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class CartesianChartHorizontalAxis : AbstractSchemaItem
{
    public const string CategoryConst = "CartesianChartHorizontalAxis";

    public CartesianChartHorizontalAxis()
        : base()
    {
        Init();
    }

    public CartesianChartHorizontalAxis(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public CartesianChartHorizontalAxis(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    #region Properties
    private string _field = "";

    [Category(category: "Field")]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "field")]
    public string Field
    {
        get { return _field; }
        set { _field = value; }
    }
    private string _caption = "";

    [Category(category: "Axis")]
    [Localizable(isLocalizable: true)]
    [XmlAttribute(attributeName: "label")]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
    private ChartAggregationType _aggregationType = ChartAggregationType.Distinct;

    [Category(category: "Limits"), DefaultValue(value: ChartAggregationType.Distinct)]
    [XmlAttribute(attributeName: "aggregationType")]
    public ChartAggregationType AggregationType
    {
        get { return _aggregationType; }
        set { _aggregationType = value; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
}
