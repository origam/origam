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

using Origam.DA.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for DeaultValueParameter.
/// </summary>
[SchemaItemDescription("Xslt Initial Value Parameter", "Parameters", "icon_xslt-initial-value-parameter.png")]
[HelpTopic("Xslt+Initial+ValueParameter")]
[ClassMetaVersion("6.0.0")]
public class XsltInitialValueParameter : SchemaItemParameter
{
    private List<OrigamDataType> osArray ;
    public XsltInitialValueParameter() : base() {
        InitArray();
    }
    private void InitArray()
    {
        osArray = new List<OrigamDataType>
        {
            OrigamDataType.Integer,
            OrigamDataType.Long,
            OrigamDataType.UniqueIdentifier,
            OrigamDataType.Currency,
            OrigamDataType.Float,
            OrigamDataType.Date,
            OrigamDataType.Boolean,
            OrigamDataType.String,
            OrigamDataType.Memo
        };
    }
    public XsltInitialValueParameter(Guid schemaExtensionId) : base(schemaExtensionId) { InitArray(); }
    public XsltInitialValueParameter(Key primaryKey) : base(primaryKey) { InitArray(); }
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(this.Transformation);
    }
    #region Properties
    //protected OrigamDataType _dataType;
    [XmlAttribute("dataType")]
    [TypeConverter(typeof(TransformOutputScalarOrigamDataTypeConverter))]
    public override OrigamDataType DataType
    {
        get
        {
            return _dataType;
        }
        set
        {
            _dataType = value;
        }
    }
    
    public Guid transformationId;
    [Category("Reference")]
    [TypeConverter(typeof(TransformationConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference("transformation", "transformationId")]
    [Description("XSLT transformation that computes a value for a parameter. The transformation can use other non-xslt parameters as an input (as <xsl:param>s). The transformation has always <ROOT/> XmlDocument as an input (data). The value for a parameter is taken from /ROOT/Value output of the transformation.")]
    public XslTransformation Transformation
    {
        get
        {
            try
            {
                return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.transformationId)) as XslTransformation;
            }
            catch
            {
                throw new Exception(ResourceUtils.GetString("ERRTransformationNotFound", this.transformationId));
            }
        }
        set
        {
            this.transformationId = (Guid)value.PrimaryKey["Id"];
        }
    }
    public List<OrigamDataType> getOrigamDataType()
    {
        return osArray;
    }
    #endregion
}
