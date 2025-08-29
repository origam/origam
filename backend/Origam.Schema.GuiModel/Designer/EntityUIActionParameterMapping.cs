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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[SchemaItemDescription("Parameter Mapping", "Parameter Mappings", "icon_parameter-mapping.png")]
[HelpTopic("Action+Parameter+Mapping")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class EntityUIActionParameterMapping : AbstractSchemaItem, IComparable
{
    public const string CategoryConst = "EntityUIActionParameterMapping";

    public EntityUIActionParameterMapping()
        : base() { }

    public EntityUIActionParameterMapping(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public EntityUIActionParameterMapping(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
    #region Properties
    private string _field = "";

    [StringNotEmptyModelElementRule()]
    [XmlAttribute("field")]
    public string Field
    {
        get { return _field; }
        set { _field = value; }
    }
    EntityUIActionParameterMappingType _type = EntityUIActionParameterMappingType.Current;

    [XmlAttribute("type")]
    public EntityUIActionParameterMappingType Type
    {
        get { return _type; }
        set { _type = value; }
    }
    #endregion
}
