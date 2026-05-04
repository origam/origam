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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel;

[ClassMetaVersion(versionStr: "6.0.0")]
public class AbstractDataDashboardWidget : AbstractDashboardWidget, IDataStructureReference
{
    public AbstractDataDashboardWidget()
        : base()
    {
        Init();
    }

    public AbstractDataDashboardWidget(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId)
    {
        Init();
    }

    public AbstractDataDashboardWidget(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.DataStructure);
        if (this.Method != null)
        {
            dependencies.Add(item: this.Method);
        }

        if (this.SortSet != null)
        {
            dependencies.Add(item: this.SortSet);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    #region Properties
    public override string Icon
    {
        get { return "79"; }
    }

    public Guid DataStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [Category(category: "Data"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "dataStructure", idField: "DataStructureId")]
    public DataStructure DataStructure
    {
        get
        {
            return (DataStructure)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DataStructureId)
                );
        }
        set
        {
            this.Method = null;
            this.SortSet = null;
            this.DataStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
        }
    }
    public Guid DataStructureMethodId;

    [Category(category: "Data"), TypeConverter(type: typeof(DataStructureReferenceMethodConverter))]
    [XmlReference(attributeName: "method", idField: "DataStructureMethodId")]
    public DataStructureMethod Method
    {
        get
        {
            return (DataStructureMethod)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DataStructureMethodId)
                );
        }
        set
        {
            this.DataStructureMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public Guid DataStructureSortSetId;

    [
        Category(category: "Data"),
        TypeConverter(type: typeof(DataStructureReferenceSortSetConverter))
    ]
    [XmlReference(attributeName: "sortSet", idField: "DataStructureSortSetId")]
    public DataStructureSortSet SortSet
    {
        get
        {
            return (DataStructureSortSet)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DataStructureSortSetId)
                );
        }
        set
        {
            this.DataStructureSortSetId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public override ArrayList Properties
    {
        get
        {
            var result = new ArrayList();
            DataStructureEntity entity =
                this.DataStructure.Entities[index: 0] as DataStructureEntity;
            foreach (DataStructureColumn column in entity.Columns)
            {
                result.Add(
                    value: new DashboardWidgetProperty(
                        name: column.Name,
                        caption: (column.Caption == null ? column.Field.Caption : column.Caption),
                        dataType: column.Field.DataType
                    )
                );
            }
            return result;
        }
    }
    #endregion
}
