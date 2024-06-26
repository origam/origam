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
using Origam.Schema.EntityModel;


namespace Origam.Schema.GuiModel;
/// <summary>
/// Summary description for AbstractDataReport.
/// </summary>
[ClassMetaVersion("6.0.0")]
public abstract class AbstractDataReport : AbstractReport
{
    public AbstractDataReport() : base() { }
    public AbstractDataReport(Guid schemaExtensionId) : base(schemaExtensionId) { }
    public AbstractDataReport(Key primaryKey) : base(primaryKey) { }
    #region Properties
    private string _reportFileName;
    public Guid DataStructureId;
    [TypeConverter(typeof(DataStructureConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("dataStructure", "DataStructureId")]
    public DataStructure DataStructure
    {
        get
        {
            return (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureId));
        }
        set
        {
            this.Method = null;
            this.SortSet = null;
            this.DataStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
        }
    }
    public Guid DataStructureMethodId;
    [TypeConverter(typeof(DataStructureReferenceMethodConverter))]
    [XmlReference("method", "DataStructureMethodId")]
    public DataStructureMethod Method
    {
        get
        {
            return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureMethodId));
        }
        set
        {
            this.DataStructureMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
        }
    }
    public Guid DataStructureSortSetId;
    [TypeConverter(typeof(DataStructureReferenceSortSetConverter))]
    [XmlReference("sortSet", "DataStructureSortSetId")]
    public DataStructureSortSet SortSet
    {
        get
        {
            return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureSortSetId));
        }
        set
        {
            this.DataStructureSortSetId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
        }
    }
    public Guid TransformationId;
    [TypeConverter(typeof(TransformationConverter))]
    [XmlReference("transformation", "TransformationId")]
    public AbstractTransformation Transformation
    {
        get
        {
            return (AbstractTransformation)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.TransformationId));
        }
        set
        {
            this.TransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
        }
    }
    [XmlAttribute("reportFileName")]
    public string ReportFileName
    {
        get
        {
            return _reportFileName;
        }
        set
        {
            _reportFileName = value;
        }
    }
    string _localeXPath;
    [Description("XPath should return locale IETF tag (e.g. en-US) to be used as current thread culture and UI culture for the report.")]
    [XmlAttribute("localeXPath")]
    public string LocaleXPath
    {
        get
        {
            return _localeXPath;
        }
        set
        {
            _localeXPath = value;
        }
    }
    #endregion
    public override void GetParameterReferences(AbstractSchemaItem parentItem, System.Collections.Hashtable list)
    {
        if (this.Method != null)
        {
            base.GetParameterReferences(this.Method as AbstractSchemaItem, list);
        }
        else
        {
            base.GetParameterReferences(this.DataStructure as AbstractSchemaItem, list);
        }
    }
    public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
    {
        dependencies.Add(this.DataStructure);
        if (this.Method != null) dependencies.Add(this.Method);
        if (this.SortSet != null) dependencies.Add(this.SortSet);
        if (this.Transformation != null) dependencies.Add(this.Transformation);
        base.GetExtraDependencies(dependencies);
    }
}
