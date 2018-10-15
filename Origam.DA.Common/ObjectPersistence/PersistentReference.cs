#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using Origam;

namespace CZ.Advantages.Asap.DA.ObjectPersistence
{
	/// <summary>
	/// Class used to reference IPersistent objects. Stores a Primary Key of the referenced
	/// object and provides a method to retrieve that object.
	/// </summary>
	public class PersistentReference
	{
		public PersistentReference(Key primaryKey, Type targetType)
		{
			this.PrimaryKey = primaryKey;
			this.TargetType = targetType;
		}

		public Key PrimaryKey;
		public Type TargetType;
	}
}
