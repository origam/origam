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

using System.Collections;
using System.Collections.Generic;
using Origam.DA;

namespace Origam.Schema.EntityModel;

public enum EntityColumnXmlMapping
{
    Element = 0,
    Attribute = 1,
    Hidden = 2,
}

/// <summary>
/// Interface for entity columns. Any expression that can represent a column will
/// implement this interface.
/// </summary>
public interface IDataEntityColumn : ISchemaItem, ICaptionSchemaItem
{
    /// <summary>
    /// Tells if this column is read only.
    /// </summary>
    bool ReadOnly { get; }
    bool ExcludeFromAllFields { get; }
    OrigamDataType DataType { get; set; }
    int DataLength { get; set; }
    bool AllowNulls { get; set; }
    bool IsPrimaryKey { get; set; }

    //string Caption{get; set;}
    IDataLookup DefaultLookup { get; set; }
    IDataEntity ForeignKeyEntity { get; set; }
    IDataEntityColumn ForeignKeyField { get; set; }
    bool AutoIncrement { get; set; }
    long AutoIncrementSeed { get; set; }
    long AutoIncrementStep { get; set; }
    DataConstant DefaultValue { get; set; }
    SchemaItemParameter DefaultValueParameter { get; set; }
    EntityColumnXmlMapping XmlMappingType { get; set; }
    OnCopyActionType OnCopyAction { get; set; }
    List<AbstractEntitySecurityRule> RowLevelSecurityRules { get; }
    List<EntityConditionalFormatting> ConditionalFormattingRules { get; }
    List<EntityFieldDynamicLabel> DynamicLabels { get; }
    DataEntityConstraint ForeignKeyConstraint { get; }

    string FieldType { get; }
}
