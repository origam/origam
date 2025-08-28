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

namespace Origam.Schema;

/// <summary>
/// Summary description for String.
/// </summary>
[SchemaItemDescription("String", "icon_string.png")]
[HelpTopic("String+Library")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class StringItem : AbstractSchemaItem
{
    public const string CategoryConst = "String";

    public StringItem()
        : base() { }

    public StringItem(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public StringItem(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
    #region Properties
    private string _string;

    [Localizable(true)]
    [XmlAttribute("string")]
    public string String
    {
        get { return _string; }
        set { _string = value; }
    }
    #endregion
}
