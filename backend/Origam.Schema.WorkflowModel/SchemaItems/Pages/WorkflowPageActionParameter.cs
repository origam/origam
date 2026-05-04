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

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(name: "Parameter", folderName: "Parameters", icon: 17)]
[HelpTopic(topic: "Action+Parameter")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkflowPageActionParameter : AbstractSchemaItem
{
    public WorkflowPageActionParameter()
        : base()
    {
        Init();
    }

    public WorkflowPageActionParameter(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public WorkflowPageActionParameter(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    public const string CategoryConst = "WorkflowPageActionParameter";

    private void Init() { }

    #region Properties
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override string Icon
    {
        get { return "17"; }
    }
    private string _xpath;

    [Category(category: "Result")]
    [Description(
        description: "An XPath expression from the context of the data returned by the workflow. The result will be used as the URL."
    )]
    [XmlAttribute(attributeName: "xPath")]
    public string XPath
    {
        get { return _xpath; }
        set { _xpath = value; }
    }
    #endregion
}
