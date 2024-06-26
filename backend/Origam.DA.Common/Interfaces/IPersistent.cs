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

namespace Origam.DA.ObjectPersistence;
/// <summary>
/// All persistable items implement this interface.
/// </summary>
public interface IPersistent : IDisposable
{
	event EventHandler Changed;
	event EventHandler Deleted;
	/// <summary>
	/// Gets or sets persistence provider to this object.
	/// </summary>
	IPersistenceProvider PersistenceProvider {get; set;}
	/// <summary>
	/// Gets primary key of the object.
	/// </summary>
	Key PrimaryKey {get;}
	Guid Id { get; }
	/// <summary>
	/// Insert or update the current object instance.
	/// </summary>
	void Persist();
	
	/// <summary>
	/// Select the current object instance and initialize all properties/fields with
	/// the values of the retrieved data. This method can be used in place of static
	/// constructors in implementations of IPersistant as it allows constructors to
	/// merely take a Key parameter (and then call this method to initialize all fields).
	/// </summary>
	void Refresh();
	/// <summary>
	/// Returns freshly retrieved copy from the persistence provider keeping the existing one
	/// as it is.
	/// </summary>
	/// <returns></returns>
	IPersistent GetFreshItem();
	/// <summary>
	/// True if the object is supposed to be deleted from the database whe Persist() is called. 
	/// Note: This does not destroy or invalidate the current object instance.
	/// </summary>
	bool IsDeleted
	{
		get;
		set;
	}
	
	/// <summary>
	/// True if the current instance has been persisted to the database.
	/// </summary>
	bool IsPersisted
	{
		get;
		set;
	}
	/// <summary>
	/// True if persistence provider's object cache can be used with this object.
	/// </summary>
	bool UseObjectCache
	{
		get;
		set;
	}
    List<string> Files
    {
        get;
    }
}
