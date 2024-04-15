#region license
/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

namespace Origam.Schema.LookupModel;

[SchemaItemDescription(
    name: "New Record Screen Binding Parameter Mapping", 
    icon: 3)]
[HelpTopic("")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("ParameterName")]
[ClassMetaVersion("6.0.0")]
public class NewRecordScreenBindingParameterMapping : AbstractSchemaItem
{
    public const string CategoryConst 
        = "NewRecordScreenBindingParameterMapping";

    public NewRecordScreenBindingParameterMapping() {}

    public NewRecordScreenBindingParameterMapping(Guid schemaExtensionId) 
        : base(schemaExtensionId) {}
    
    public NewRecordScreenBindingParameterMapping(Key primaryKey) 
        : base(primaryKey) {}
    
    #region Overriden AbstractSchemaItem Members
    public override string ItemType => CategoryConst;

    public override SchemaItemCollection ChildItems => new();

    #endregion
    
    #region Properties
    
    [Category("Data")]
    [NotNullModelElementRule]
    [XmlAttribute("parameterName")]
    [Description("Parameter used in (added to) the list data structure of the lookup. The referenced source column will be set in the properties of the screen section combo box where the lookup is used. If the actual text content of the lookup field should be copied, the value should be set to SearchText.")]
    public string ParameterName { get; set; }
    
    [Category("Data")]
    [NotNullModelElementRule]
    [XmlAttribute("targetRootEntityField")]
    [Description("Column in target root entity to be filled.")]
    public string TargetRootEntityField { get; set; }
		
    #endregion
}
