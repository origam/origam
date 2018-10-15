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

namespace Origam.Schema
{
	/// <summary>
	/// Use this attribute to specify a name and folder name of the schema item.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple=false, Inherited=true )]
	public class SchemaItemDescriptionAttribute : Attribute
	{
		private string _name;
		private string _folderName;
		private int _icon;

		public SchemaItemDescriptionAttribute(string name, int icon)
		{
			this._name = name;
			this._icon = icon;
		}

		public SchemaItemDescriptionAttribute(string name, string folderName, int icon) : this(name, icon)
		{
			this._folderName = folderName;
		}

		public string Name 
		{
			get{return _name ?? GetType().Name;}
		}

		public string FolderName
		{
			get{return _folderName;}
		}

		public int Icon
		{
			get{return _icon;}
		}
	}
}
