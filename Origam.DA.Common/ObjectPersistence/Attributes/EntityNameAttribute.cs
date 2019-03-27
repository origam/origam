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
	/// Use this attribute on classes that should be persistable. Only classes decorated
	/// with this attribute will be supported by Origam object persistence.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple=false, Inherited=true )]
	public sealed class EntityNameAttribute : Attribute
	{
		private string _name;
		private string _inheritanceColumn;

		/// <summary>
		/// The constructor for the TableName attribute.
		/// </summary>
		/// <param name="name">The name of the database table used to store instances of this class.</param>
		public EntityNameAttribute(string name)
		{
			this._name = name;
		}

		public EntityNameAttribute(string name, string inheritanceColumn) : this(name)
		{
			this._inheritanceColumn = inheritanceColumn;
		}

		/// <summary>
		/// The name of the data structure entity used to store instances of this class.
		/// </summary>
		public string Name 
		{
			get{return _name;}
		}

		/// <summary>
		/// The name of the column in this entity, where target object type is stored.
		/// </summary>
		public string InheritanceColumn 
		{
			get{return _inheritanceColumn;}
		}

		/// <summary>
		/// Returns True if the table is a container for different object types.
		/// </summary>
		public bool IsInherited
		{
			get{return (_inheritanceColumn == "");}
		}
	}
}
