#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

namespace Origam.DA.ObjectPersistence
{
	/// <summary>
	/// Use this attribute to identify the properties of classes. Only attributes decorated
	/// with this attribute will be supported by Origam object persistence.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
	public sealed class EntityColumnAttribute : Attribute
	{
		private string _name;
		private string _entityName = null;
		private bool _isForeignKey = false;

		/// <summary>
		/// The constructor for the EntityColumn attribute.
		/// </summary>
		/// <param name="name">The name of the column used to store attributes of this class.</param>
		public EntityColumnAttribute(string name)
		{
			this._name = name;
		}

		public EntityColumnAttribute(string name, bool isForeignKey) : this(name)
		{
			this._isForeignKey = isForeignKey;
		}

		/// <summary>
		/// The constructor for EntityColumn attribute, which allows overriding entity name.
		/// </summary>
		/// <param name="name">The name of the column used to store attributes of this class.</param>
		/// <param name="overridenTableName">The name of another entity where the column is located.
		/// This entity is looked up by the same primary key as the main entity.</param>
		public EntityColumnAttribute(string name, string overridenEntityName) : this(name)
		{
			this._entityName = overridenEntityName;
		}

		public EntityColumnAttribute(string name, string overridenEntityName, bool isForeignKey) : this(name, isForeignKey)
		{
			this._entityName = overridenEntityName;
		}
		
		/// <summary>
		/// The name of the DataStructure's entity column used to store instances of this class.
		/// </summary>
		public string Name 
		{
			get{return _name;}
		}

		/// <summary>
		/// The name of the DataStructure's entity column used to store instances of this class.
		/// </summary>
		public string OverridenEntityName 
		{
			get{return _entityName;}
		}

		/// <summary>
		/// The value is of type IPersistent and its ForeignKey will be used to persist.
		/// "Name" will be used as a prefix to all the primary key columns of the referred
		/// object.
		/// </summary>
		/// <example>
		///	My entity name is "Invoice".
		///	Foreign entity name is "Customers" and has primary key "Id".
		///	
		///	In class "Invoice" we have a property "Customer" (IPersistent) which we want 
		///	to persist to the "Invoice" table.
		///	
		///	We use attribute [EntityColumn("refCustomer", true)].
		///	
		///	This will result in "refCustomerId" (prefix + key of the foreign table) to be
		///	used as a column to store the foreign key.
		///	</example>
		public bool IsForeignKey
		{
			get{return _isForeignKey;}
		}
	}
}
