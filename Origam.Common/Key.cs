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
using System.Collections;

namespace Origam
{
	/// <summary>
	/// This class represents a collection of column names/values that make a unique key of an entity.
	/// </summary>
	public class Key : Hashtable
	{
		public Key()
		{
		}

	    public Key(Guid dataStructureEntityId)
	    {
	        this["Id"] = dataStructureEntityId;
	    }

        public override string ToString()
		{
			string keyString = "";

			foreach(DictionaryEntry entry in this)
			{
				keyString = keyString + entry.Value.ToString();
			}

			return keyString;
		}

		public object[] ValueArray
		{
			get
			{
				object[] ret = new object[this.Values.Count];

				this.Values.CopyTo(ret, 0);

				return ret;
			}
		}

		public object[] KeyArray
		{
			get
			{
				object[] ret = new object[this.Values.Count];

				this.Keys.CopyTo(ret, 0);

				return ret;
			}
		}

		public override bool Equals(object obj)
		{
			if(obj is Key)
			{
				Key refKey = obj as Key;

				if(this.Count != refKey.Count) return false;

				foreach(object key in this.Keys)
				{
					//if(this[key].ToString() != refKey[key].ToString())
					if(!(this[key].Equals(refKey[key])))
						return false;
				}
			}
			else
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;

			foreach(object key in this.Keys)
			{
				hashCode = hashCode ^ key.GetHashCode();
			}

			return hashCode;
		}

	}
}
