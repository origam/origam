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
using System.Drawing;

namespace Origam.UI;

/// <summary>
/// Summary description for IBrowserNode.
/// </summary>
public interface IBrowserNode2 : IBrowserNode
{
	bool CanDelete
	{
		get;
	}

	void Delete();

	bool CanMove(IBrowserNode2 newNode);
		
	IBrowserNode2 ParentNode {get; set;}

	byte[] NodeImage {get;}

	bool Hide{get; set;}

	string NodeId{get;}

	string FontStyle { get; }
}

public interface IBrowserNode : IComparable
{
	/// <summary>
	/// Gets all nodes supposed to be displayed under this node.
	/// </summary>
	/// <returns></returns>
	BrowserNodeCollection ChildNodes();
		
	/// <summary>
	/// Displayed text of the node in the user interface.
	/// </summary>
	string NodeText
	{
		get;
		set;
	}

	/// <summary>
	/// Path to an icon of the node.
	/// </summary>
	string Icon
	{
		get;
	}

	/// <summary>
	/// True if node has children.
	/// </summary>
	bool HasChildNodes
	{
		get;
	}

	/// <summary>
	/// True if node supports renaming through changing NodeText property.
	/// </summary>
	bool CanRename
	{
		get;
	}
}