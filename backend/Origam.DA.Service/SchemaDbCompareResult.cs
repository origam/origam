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

using Origam.Schema;
using static Origam.DA.Common.Enums;

namespace Origam.DA;
public enum DbCompareResultType
{
	MissingInDatabase = 0,
	MissingInSchema = 1,
	ExistingButDifferent = 2
}
/// <summary>
/// Summary description for SchemaCompareResult.
/// </summary>
public class SchemaDbCompareResult
{
	public SchemaDbCompareResult()
	{
	}
	#region Properties
	private ISchemaItem _schemaItem;
	public ISchemaItem SchemaItem
	{
		get
		{
			return _schemaItem;
		}
		set
		{
			_schemaItem = value;
		}
	}
	private Type _schemaItemType;
	public Type SchemaItemType
	{
		get
		{
			return _schemaItemType;
		}
		set
		{
			_schemaItemType = value;
		}
	}
	private object _parentSchemaItem;
	public object ParentSchemaItem
	{
		get
		{
			return _parentSchemaItem;
		}
		set
		{
			_parentSchemaItem = value;
		}
	}
	private DbCompareResultType _resultType;
	public DbCompareResultType ResultType
	{
		get
		{
			return _resultType;
		}
		set
		{
			_resultType = value;
		}
	}
	private string _itemName;
	public string ItemName
	{
		get
		{
			return _itemName;
		}
		set
		{
			_itemName = value;
		}
	}
	private string _remark = "";
	public string Remark
	{
		get
		{
			return _remark;
		}
		set
		{
			_remark = value;
		}
	}
	private string _script = "";
	public string Script
	{
		get
		{
			return _script;
		}
		set
		{
			_script = value;
		}
	}
	private string _script2 = "";
	public string Script2
	{
		get
		{
			return _script2;
		}
		set
		{
			_script2 = value;
		}
	}
    public Platform Platform { get; set; }
    #endregion
}
