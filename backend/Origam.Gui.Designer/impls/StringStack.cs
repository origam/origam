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

//------------------------------------------------------------------------------
/// <copyright from='1997' to='2002' company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///
///    This source code is intended only as a supplement to Microsoft
///    Development Tools and/or on-line documentation.  See these other
///    materials for detailed information regarding Microsoft code samples.
///
/// </copyright>
//------------------------------------------------------------------------------
namespace Origam.Gui.Designer;

using System.Collections;

/// This is just a special stack to handle the transaction descriptions.
/// It functions like a normal stack, except it looks for the first
/// non-null (and non "") string.
internal class StringStack : Stack 
{
	internal StringStack() 
	{
		}

	internal string GetNonNull() 
	{
			int items = this.Count;
			object item;
			object[] itemArr = this.ToArray();
			for (int i = items - 1; i >=0; i--) 
			{
				item = itemArr[i];
				if (item != null && item is string && ((string)item).Length > 0) 
				{
					return (string)item;
				}
			}
			return "";
		}
}