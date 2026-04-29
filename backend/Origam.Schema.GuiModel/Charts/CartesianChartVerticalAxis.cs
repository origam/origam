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
    name: "Vertical Axis",
    folderName: "Vertical Axes",
    iconName: "icon_vertical-axis.png"
)]
[HelpTopic(topic: "Vertical+Axis")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class CartesianChartVerticalAxis : AbstractSchemaItem
{
    public const string CategoryConst = "CartesianChartVerticalAxis";

    public CartesianChartVerticalAxis()
        : base()
    {
        Init();
    }

    public CartesianChartVerticalAxis(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public CartesianChartVerticalAxis(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    #region Properties
    private string _caption = "";

    [Category(category: "Axis")]
    [Localizable(isLocalizable: true)]
    [XmlAttribute(attributeName: "label")]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
    private int _min = 0;

    [Category(category: "Limits"), DefaultValue(value: 0)]
    [XmlAttribute(attributeName: "minimum")]
    public int Min
    {
        get { return _min; }
        set { _min = value; }
    }
    private int _max = 0;

    [Category(category: "Limits"), DefaultValue(value: 0)]
    [XmlAttribute(attributeName: "maximum")]
    public int Max
    {
        get { return _max; }
        set { _max = value; }
    }
    private bool _applyMin = false;

    [Category(category: "Limits"), DefaultValue(value: false)]
    [XmlAttribute(attributeName: "applyMinimumLimit")]
    public bool ApplyMinLimit
    {
        get { return _applyMin; }
        set { _applyMin = value; }
    }
    private bool _applyMax = false;

    [Category(category: "Limits"), DefaultValue(value: false)]
    [XmlAttribute(attributeName: "applyMaximumLimit")]
    public bool ApplyMaxLimit
    {
        get { return _applyMax; }
        set { _applyMax = value; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
}
