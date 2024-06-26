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
using System.ComponentModel;

using Origam.UI;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using Origam.DA.Common;

namespace Origam.Schema.DeploymentModel;
/// <summary>
/// Summary description for AbstractUpdateScriptActivity.
/// </summary>
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public abstract class AbstractUpdateScriptActivity : AbstractSchemaItem, IComparable
{
	public const string CategoryConst = "DeploymentUpdateScriptActivity";
	public AbstractUpdateScriptActivity() : base() {}
	public AbstractUpdateScriptActivity(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public AbstractUpdateScriptActivity(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override bool CanMove(IBrowserNode2 newNode)
	{
		DeploymentVersion newVersion = newNode as DeploymentVersion;
		if(newVersion != null)
		{
			return true;
		}
		return base.CanMove (newNode);
	}
	#endregion
	#region Properties
	private int _activityOrder;
	[Category("Update Script Activity")]
	[XmlAttribute("activityOrder")]
	public int ActivityOrder
	{
		get
		{
			return _activityOrder;
		}
		set
		{
			_activityOrder = value;
		}
	}
	internal DeploymentVersion Version
	{
		get
		{
			return this.ParentItem as DeploymentVersion;
		}
	}
	#endregion
	#region IComparable Members
	public override int CompareTo(object obj)
	{
		if(obj is AbstractUpdateScriptActivity)
		{
			AbstractUpdateScriptActivity compared = obj as AbstractUpdateScriptActivity;
			Version n = new Version(this.Version.VersionString);
			Version o = new Version(compared.Version.VersionString);
			int versionCompare = n.CompareTo(o);
			if(versionCompare == 0)
			{
				return this.ActivityOrder.CompareTo(compared.ActivityOrder);
			}
			else
			{
				return versionCompare;
			}
		}
		else
		{
			throw new ArgumentOutOfRangeException("obj", obj, ResourceUtils.GetString("ErrorCompare"));
		}
	}
	#endregion
}
