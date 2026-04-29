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
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription(
    name: "Screen Mapping",
    folderName: "Screen Mappings",
    iconName: "icon_screen-mapping.png"
)]
[HelpTopic(topic: "Chart+Screen+Mapping")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ChartFormMapping : AbstractSchemaItem
{
    public const string CategoryConst = "ChartFormMapping";

    public ChartFormMapping()
        : base()
    {
        Init();
    }

    public ChartFormMapping(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public ChartFormMapping(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    private void UpdateName()
    {
        if (this.Screen != null)
        {
            this.Name = this.Screen.Name;
            if (this.Entity != null)
            {
                this.Name += "_" + this.Entity.Name;
            }
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Screen);
        dependencies.Add(item: this.Entity);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    #region Properties
    public Guid ScreenId;

    [Category(category: "Screen Reference")]
    [TypeConverter(type: typeof(FormControlSetConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "screen", idField: "ScreenId")]
    public FormControlSet Screen
    {
        get
        {
            return (FormControlSet)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.ScreenId)
                );
        }
        set { this.ScreenId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }
    public Guid EntityId;

    [Category(category: "Screen Reference")]
    [TypeConverter(type: typeof(ChartFormMappingEntityConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "entity", idField: "EntityId")]
    public DataStructureEntity Entity
    {
        get
        {
            return (DataStructureEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.EntityId)
                );
        }
        set { this.EntityId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
}
