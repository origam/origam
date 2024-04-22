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

using System.Collections;

namespace Origam.Schema.EntityModel;

public enum EntityAuditingType
{
	None = 0,
	All = 1,
	UpdatesAndDeletes = 2
}

/// <summary>
/// Interface for data entities, which contain columns
/// </summary>
public interface IDataEntity : ISchemaItem
{
	/// <summary>
	/// Parameters of this data entity (in case the entity is query
	/// </summary>
	ArrayList EntityParameters {get;}

	/// <summary>
	/// Collection of expressions which make up a primary key of this data entity
	/// </summary>
	ArrayList EntityPrimaryKey {get;}
		
	/// <summary>
	/// Returns true if complete data entity is read only
	/// </summary>
	bool EntityIsReadOnly {get; set;}

	/// <summary>
	/// Returns all columns of this data entity
	/// </summary>
	ArrayList EntityColumns {get;}

	/// <summary>
	/// Returns all relations of this data entity
	/// </summary>
	ArrayList EntityRelations {get;}

	/// <summary>
	/// Returns all filters of this data entity
	/// </summary>
	ArrayList EntityFilters {get;}

	/// <summary>
	/// Returns all indexes of this data entity
	/// </summary>
	ArrayList EntityIndexes {get;}

	/// <summary>
	/// Returns all security rules of this data entity
	/// </summary>
	ArrayList RowLevelSecurityRules {get;}

	ArrayList ConditionalFormattingRules {get;}

	ArrayList Constraints {get;}

	string Caption {get; set;}

	EntityAuditingType AuditingType {get; set;}

	IDataEntityColumn AuditingSecondReferenceKeyColumn { get; set; }

	ArrayList ChildEntitiesRecursive {get;}
	ArrayList ChildEntities {get;}

	IDataEntityColumn DescribingField {get; set;}
	bool HasEntityAFieldDenyReadRule();
}