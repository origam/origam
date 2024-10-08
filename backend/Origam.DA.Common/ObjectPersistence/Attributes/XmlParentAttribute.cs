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

namespace Origam.DA.ObjectPersistence;
/// <summary>
/// Use this attribute to identify a Guid type field which stores a reference
/// to a parent object. This value will not be serialized to the model storage
/// but when retrieving it will be passed from a parent object.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
public sealed class XmlParentAttribute : Attribute
{
	private Type _type;
    /// <summary>
    /// The constructor for the XmlParent attribute.
    /// </summary>
    /// <param name="type">The type that is referenced.</param>
    public XmlParentAttribute(Type type)
	{
		_type = type;
	}
	/// <summary>
	/// The type referenced.
	/// </summary>
	public Type Type 
	{
		get{return _type;}
	}
}
