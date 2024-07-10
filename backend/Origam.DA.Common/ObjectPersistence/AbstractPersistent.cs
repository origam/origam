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

using Origam.Git;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Origam.DA.ObjectPersistence;
/// <summary>
/// Abstract class the developer can implement to save time on developing persistent classes.
/// The final class must have a constructor with (Key) parameter, that has to call the same
/// abstract constructor here.
/// The final call also has to add the right keys to the collection (in its default constructor).
/// </summary>
public class AbstractPersistent : IPersistent
{
	public AbstractPersistent()
	{
//			Debug.UpdateCounter("SchemaItemCount", 1);
	}
//		~AbstractPersistent()
//		{
//			Dispose(false);
//
//			//System.Diagnostics.Debug.WriteLine("Persistent object destructed");
//			Debug.UpdateCounter("SchemaItemCount", -1);
//		}
	/// <summary>
	/// This constructor assigns a primary key and checks if this primary key is valid in the
	/// object that inherits this class.
	/// </summary>
	/// <param name="primaryKey">Primary key assigned to this object</param>
	/// <param name="correctKeys">String array of column names composing the correct primary key</param>
	public AbstractPersistent(Key primaryKey, Array correctKeys) : this()
	{
		foreach(string key in correctKeys)
		{
			if(!primaryKey.ContainsKey(key))
				throw new ArgumentOutOfRangeException("primaryKey", primaryKey, ResourceUtils.GetString("NoKeyInPrimaryKey", key));
		}
		if(primaryKey.Count != correctKeys.GetLength(0))
			throw new ArgumentOutOfRangeException("primaryKey", primaryKey, ResourceUtils.GetString("InvalidNumberKeys"));
		_primaryKey = primaryKey;
	}
	#region IPersistent Members
	private bool _isDeleted = false;
	[Browsable(false)] public virtual bool IsDeleted
	{
		get
		{
			return _isDeleted;
		}
		set
		{
			_isDeleted = value;
		}
	}
	Key _primaryKey = new Key();
	[Browsable(false)] public virtual Key PrimaryKey
	{
		get => _primaryKey;
		set => _primaryKey = value;
	}
	public Guid Id => (Guid) PrimaryKey["Id"];
	public virtual void Persist()
	{
		bool isNew = (! this.IsPersisted);
		this.PersistenceProvider.Persist(this);
	    if (IsDeleted)
	    {
	        OnDeleted(EventArgs.Empty);
	        PersistenceProvider.OnTransactionEnded(this);
        }
        GitManager.PersistPath(Files);
        //HasGitChange = null;
        if (isNew) OnChanged(EventArgs.Empty);
	}
	private bool _isPersisted = false;
	[Browsable(false)] 
	public bool IsPersisted
	{
		get
		{
			return _isPersisted;
		}
		set
		{
			_isPersisted = value;
			if(! value)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine(false, "IsPersisted");
#endif
			}
		}
	}
	private bool _useObjectCache = true;
	[Browsable(false)] 
	public virtual bool UseObjectCache
	{
		get
		{
			return _useObjectCache;
		}
		set
		{
			_useObjectCache = value;
		}
	}
	public virtual void Refresh()
	{
		this.PersistenceProvider.RefreshInstance(this);
		OnChanged(EventArgs.Empty);
	}
	public virtual IPersistent GetFreshItem()
	{
		IPersistent freshItem = this.PersistenceProvider.RetrieveInstance(this.GetType(), this.PrimaryKey, false) as IPersistent;
		return freshItem;
	}
	private IPersistenceProvider _persistenceProvider = null;
	[Browsable(false)] 
    public virtual IPersistenceProvider PersistenceProvider
	{
		get
		{
			return _persistenceProvider;
		}
		set
		{
			_persistenceProvider = value;
		}
	}
    [Browsable(false)]
    public List<string> Files => _persistenceProvider.Files(this);
    #endregion
    #region IDisposable Members
    public void Dispose()
	{
#if DEBUG
		System.Diagnostics.Debug.WriteLine("Persistent object disposed");
#endif
		Dispose(true);
		GC.SuppressFinalize(this);	 // Finalization is now unnecessary
	}
	private bool _disposed = false;
	protected virtual void Dispose(bool disposing)
	{
		if(!_disposed)
		{
			if(disposing)
			{
				_persistenceProvider = null;
			}
     
			// Dispose unmanaged resources
			_disposed = true;
		}
  
	}
	#endregion
	public event EventHandler Deleted;
	void OnDeleted(EventArgs e)
	{
		if (Deleted != null) 
		{
			Deleted(this, e);
		}
	}
	public event EventHandler Changed;
	void OnChanged(EventArgs e)
	{
		if (Changed != null) 
		{
			Changed(this, e);
		}
	}
}
