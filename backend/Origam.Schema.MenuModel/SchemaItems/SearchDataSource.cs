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
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using System.Collections;
using System.Xml.Serialization;

namespace Origam.Schema.MenuModel;
/// <summary>
/// Summary description for Graphics.
/// </summary>
[SchemaItemDescription("Search Data Source", 9)]
[HelpTopic("Search Data Sources")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class SearchDataSource : AbstractSchemaItem, IDataStructureReference
{
	public const string CategoryConst = "SearchDataSource";
    public SearchDataSource() : base() { Init(); }
    public SearchDataSource(Guid schemaExtensionId) : base(schemaExtensionId) { Init(); }
    public SearchDataSource(Key primaryKey) : base(primaryKey) { Init(); }
    private void Init()
    {
    }
    #region Properties
    public Guid DataStructureId;
    [TypeConverter(typeof(DataStructureConverter))]
    [NotNullModelElementRule()]
    [Category("Data Source")]
    [DisplayName("Data Structure")]
    [Description("Data structure that will be used to retrieve search results. Must contain columns: Name, ReferenceId; optional column: Description.")]
    [XmlReference("dataStructure", "DataStructureId")]
    public DataStructure DataStructure
    {
        get
        {
            return (DataStructure)this.PersistenceProvider.RetrieveInstance(
                typeof(DataStructure), new ModelElementKey(this.DataStructureId));
        }
        set
        {
            this.DataStructureId = (Guid)value.PrimaryKey["Id"];
        }
    }
    public Guid DataStructureMethodId;
    [TypeConverter(typeof(DataStructureReferenceMethodConverter))]
    [Category("Data Source")]
    [DisplayName("Data Structure Method")]
    [Description("Method that will accept a string parameter to return search results.")]
    [NotNullModelElementRule()]
    [XmlReference("method", "DataStructureMethodId")]
    public DataStructureMethod Method
    {
        get
        {
            return (DataStructureMethod)PersistenceProvider.RetrieveInstance(
                typeof(AbstractSchemaItem), 
                new ModelElementKey(DataStructureMethodId));
        }
        set
        {
            DataStructureMethodId = (value == null ? Guid.Empty : 
                (Guid)value.PrimaryKey["Id"]);
        }
    }
    private string _groupLabel;
    [Category("Results")]
    [DisplayName("Group Label")]
    [Description("A text under which the search results will be grouped.")]
    [NotNullModelElementRule()]
    [Localizable(true)]
    [XmlAttribute("groupLabel")]
    public string GroupLabel
    {
        get
        {
            return _groupLabel;
        }
        set
        {
            _groupLabel = value;
        }
    }
    private string _filterParameter;
    [Category("Data Source")]
    [DisplayName("Filter Parameter")]
    [Description("String parameter that will accept the searched text.")]
    [NotNullModelElementRule()]
    [XmlAttribute("filterParameter")]
    public string FilterParameter
    {
        get
        {
            return _filterParameter;
        }
        set
        {
            _filterParameter = value;
        }
    }
    public Guid LookupId;
    [Category("Reference")]
    [TypeConverter(typeof(DataLookupConverter))]
    [NotNullModelElementRule()]
    [XmlReference("lookup", "LookupId")]
    public IDataLookup Lookup
    {
        get
        {
            return (IDataLookup)this.PersistenceProvider.RetrieveInstance(
                typeof(AbstractSchemaItem), new ModelElementKey(this.LookupId));
        }
        set
        {
            this.LookupId = (value == null ? Guid.Empty 
                : (Guid)value.PrimaryKey["Id"]);
        }
    }
    private string _roles = "*";
    [Category("Security")]
    [NotNullModelElementRule()]
    [XmlAttribute("roles")]
    public string Roles
    {
        get
        {
            return _roles;
        }
        set
        {
            _roles = value;
        }
    }
    #endregion
    #region Overriden AbstractSchemaItem Members
    public override void GetExtraDependencies(ArrayList dependencies)
    {
        dependencies.Add(this.DataStructure);
        base.GetExtraDependencies(dependencies);
    }
    public override bool UseFolders
    {
        get
        {
            return false;
        }
    }
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override string Icon
	{
		get
		{
			return "9";
		}
	}
	#endregion
}
