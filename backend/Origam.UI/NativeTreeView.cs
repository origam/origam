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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MoreLinq;

namespace Origam.UI;
/// <summary>
/// Summary description for NativeTreeView.
/// </summary>
public class NativeTreeView : System.Windows.Forms.TreeView
{
	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
	private extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName,
		string pszSubIdList);
	
	HashSet<string> expandedPaths = new HashSet<string>();
	public TreeNode RootNode {
		get
		{
			if (TopNode == null) return null;
			TreeNode node = TopNode;
			while (node.Parent != null)
			{
				node = node.Parent;
			}
			return node;
		}
	}
	
	protected override void CreateHandle()
	{
		if(! this.IsDisposed)
		{
			base.CreateHandle();
			SetWindowTheme(this.Handle, "explorer", null);
		}
	}
	
	public void ReExpand()
	{
		StoreExpansionState();
		CollapseAllNodes();
		RestoreExpansionState();
	}
	private void CollapseAllNodes()
	{
		RootNode?.Collapse();
	}
	private IEnumerable<TreeNode> GetAllNodes() => GetAllNodes(Nodes);
	public void RestoreExpansionState()
	{
		GetAllNodes()
			.Where(node => expandedPaths.Contains(node.FullPath))
			.ForEach(node => node.Expand());
	}
	public void StoreExpansionState()
	{
		expandedPaths.Clear();
		GetAllNodes()
			.Where(node => node.IsExpanded)
			.Select(node => node.FullPath)
			.ForEach(treePath => expandedPaths.Add(treePath));
	}
		
	private IEnumerable<TreeNode> GetAllNodes(TreeNodeCollection nodes)
	{
		foreach (object nodeObj in nodes)
		{
			TreeNode node =(TreeNode) nodeObj;
			yield return node;
			foreach (TreeNode childNode in GetAllNodes(node.Nodes))
			{
				yield return childNode;
			}
		}
	}
}
