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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for AbstractDataReport.
/// </summary>
[ClassMetaVersion(versionStr: "6.0.0")]
public abstract class AbstractDataReport : AbstractReport
{
    public AbstractDataReport()
        : base() { }

    public AbstractDataReport(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public AbstractDataReport(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    private string _reportFileName;
    public Guid DataStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
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

    [TypeConverter(type: typeof(DataStructureReferenceMethodConverter))]
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

    [TypeConverter(type: typeof(DataStructureReferenceSortSetConverter))]
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
    public Guid TransformationId;

    [TypeConverter(type: typeof(TransformationConverter))]
    [XmlReference(attributeName: "transformation", idField: "TransformationId")]
    public AbstractTransformation Transformation
    {
        get
        {
            return (AbstractTransformation)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.TransformationId)
                );
        }
        set
        {
            this.TransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
        }
    }

    [XmlAttribute(attributeName: "reportFileName")]
    public string ReportFileName
    {
        get { return _reportFileName; }
        set { _reportFileName = value; }
    }
    string _localeXPath;

    [Description(
        description: "XPath should return locale IETF tag (e.g. en-US) to be used as current thread culture and UI culture for the report."
    )]
    [XmlAttribute(attributeName: "localeXPath")]
    public string LocaleXPath
    {
        get { return _localeXPath; }
        set { _localeXPath = value; }
    }
    #endregion
    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.Method != null)
        {
            base.GetParameterReferences(parentItem: Method, list: list);
        }
        else
        {
            base.GetParameterReferences(parentItem: DataStructure, list: list);
        }
    }

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

        if (this.Transformation != null)
        {
            dependencies.Add(item: this.Transformation);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }
}
