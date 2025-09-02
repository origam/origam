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

namespace Origam.Schema.EntityModel;

public enum OrigamFunctionType
{
    Standard = 0,
    Database = 1,
}

[SchemaItemDescription("Function", "icon_10_functions.png")]
[HelpTopic("Functions")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class Function : AbstractSchemaItem
{
    public const string CategoryConst = "Function";

    public Function() { }

    public Function(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public Function(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType => CategoryConst;

    [Browsable(false)]
    public override bool UseFolders => false;
    public override string Icon
    {
        get
        {
            switch (FunctionType)
            {
                case OrigamFunctionType.Database:
                    return "55";
                default:
                    return "icon_10_functions.png";
            }
        }
    }
    #endregion
    #region ISchemaItemFactory Members
    [Browsable(false)]
    public override Type[] NewItemTypes => new[] { typeof(FunctionParameter) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        return base.NewItem<T>(
            schemaExtensionId,
            group,
            typeof(T) == typeof(FunctionParameter) ? "NewFunctionParameter" : null
        );
    }
    #endregion
    #region Properties
    private OrigamDataType _dataType;

    [XmlAttribute("dataType")]
    public OrigamDataType DataType
    {
        get => _dataType;
        set => _dataType = value;
    }
    private OrigamFunctionType _functionType;

    [XmlAttribute("type")]
    public OrigamFunctionType FunctionType
    {
        get => _functionType;
        set => _functionType = value;
    }
    #endregion
}
