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
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for EntityRelationItem.
/// </summary>
[SchemaItemDescription("Data Structure", "icon_data-structure.png")]
[HelpTopic("Data+Structures")]
public class DataStructure : AbstractDataStructure, ISchemaItemFactory
{
	public DataStructure() : base(){ Init(); }
	
	public DataStructure(Guid schemaExtensionId) : base(schemaExtensionId) { Init(); }
	public DataStructure(Key primaryKey) : base(primaryKey)	{ Init(); }
	#region Properties
	[Browsable(false)]
	public ArrayList Entities
	{
		get
		{
			ArrayList entities = new ArrayList();
			foreach(DataStructureEntity entity in this.ChildItemsByType(DataStructureEntity.CategoryConst))
			{
				entities.Add(entity);
				entities.AddRange(GetChildEntities(entity));
			}
			return entities;
		}
	}
	[Browsable(false)]
    public IList<DataStructureEntity> LocalizableEntities
    {
        get
        {
            List<DataStructureEntity> result = new List<DataStructureEntity>();
            foreach (DataStructureEntity dsEntity in Entities)
            {
                TableMappingItem table = dsEntity.Entity as TableMappingItem;
                if (table != null && table.LocalizationRelation != null)
                {
                    result.Add(dsEntity);
                }
            }
            return result;
        }
    }
	[Browsable(false)]
	public List<ISchemaItem> DefaultSets
	{
		get
		{
			return this.ChildItemsByType(DataStructureDefaultSet.CategoryConst);
		}
	}
	[Browsable(false)]
	public List<ISchemaItem> TemplateSets
	{
		get
		{
			return this.ChildItemsByType(DataStructureTemplateSet.CategoryConst);
		}
	}
	[Browsable(false)]
	public List<ISchemaItem> Methods
	{
		get
		{
			return this.ChildItemsByType(DataStructureMethod.CategoryConst);
		}
	}
	[Browsable(false)]
	public List<ISchemaItem> RuleSets
	{
		get
		{
			return this.ChildItemsByType(DataStructureRuleSet.CategoryConst);
		}
	}
	[Browsable(false)]
	public List<ISchemaItem> SortSets
	{
		get
		{
			return this.ChildItemsByType(DataStructureSortSet.CategoryConst);
		}
	}
	private ArrayList GetChildEntities(DataStructureEntity entity)
	{
		ArrayList entities = new ArrayList();
		foreach(DataStructureEntity childEntity in entity.ChildItemsByType(DataStructureEntity.CategoryConst))
		{
			entities.Add(childEntity);
			entities.AddRange(GetChildEntities(childEntity));
		}
		return entities;
	}
	private bool _isLocalized = false;
	[Description("Translate data for all entities, that has realtion marked with IsMultilingual='true'. If set to true, any read-write operation will fail.")]
    [XmlAttribute("localized")]
	public bool IsLocalized
	{
		get
		{
			return _isLocalized;
		}
		set
		{
			_isLocalized = value;
		}
	}
	private string _dataSetClass;
	[Description("A fully qualified name of a class followed by an assembly name which has a class in it. A class should correspond (should have same xsd) as a xsd of a current datastructure. A class will be used everytime a dataset is to be created from a datastructure. A class is worth defining when we need to seamlessly pass a dataset between origam and a service agent (library) code.")]
    [XmlAttribute("dataSetClass")]
    public string DataSetClass
	{
		get
		{
			return _dataSetClass;
		}
		set
		{
			_dataSetClass = value;
		}
	}
	#endregion
	#region Overriden AbstractSchemaItem Members
	private void Init()
	{
		this.ChildItemTypes.InsertRange(0,
			new Type[] {
						   typeof(DataStructureEntity),
						   typeof(DataStructureFilterSet),
						   typeof(DataStructureDefaultSet),
						   typeof(DataStructureTemplateSet),
						   typeof(DataStructureRuleSet),
						   typeof(DataStructureSortSet),
						   typeof(SchemaItemParameter)
					   }
			   );                        
	}
	public override void GetParameterReferences(AbstractSchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		foreach(DataStructureEntity item in Entities)
		{
			item.GetParameterReferences(item, list);
		}
		foreach(DataStructureDefaultSet defset in DefaultSets)
		{
			defset.GetParameterReferences(defset, list);
		}
	}
	#endregion
}
