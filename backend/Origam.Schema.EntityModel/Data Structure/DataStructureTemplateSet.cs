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

namespace Origam.Schema.EntityModel;

[SchemaItemDescription(
    name: "Template Set",
    folderName: "Template Sets",
    iconName: "icon_template-set.png"
)]
[HelpTopic(topic: "Template+Sets")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureTemplateSet : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureTemplateSet";

    public DataStructureTemplateSet() { }

    public DataStructureTemplateSet(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public DataStructureTemplateSet(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    #region Properties
    [Browsable(browsable: false)]
    public List<DataStructureTemplate> Templates =>
        ChildItemsByType<DataStructureTemplate>(itemType: DataStructureTemplate.CategoryConst);
    #endregion
    #region Public Methods
    public List<DataStructureTemplate> TemplatesByDataMember(string dataMember)
    {
        var result = new List<DataStructureTemplate>();
        foreach (DataStructureTemplate template in Templates)
        {
            if (template.Entity.Name == dataMember)
            {
                result.Add(item: template);
            }
        }
        return result;
    }
    #endregion
    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType => CategoryConst;
    public override bool UseFolders => false;
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes => new[] { typeof(DataStructureTransformationTemplate) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: typeof(T) == typeof(DataStructureTransformationTemplate)
                ? "NewTransformationTemplate"
                : null
        );
    }
    #endregion
}
