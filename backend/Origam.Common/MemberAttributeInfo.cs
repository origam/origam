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

/*
 * Helper class for storing information on members and their attributes
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Origam;
public class MemberAttributeInfo
{
	public readonly MemberInfo MemberInfo;
	public readonly Attribute Attribute;
	public readonly List<Attribute> MemberAttributes;

	public MemberAttributeInfo( MemberInfo memberInfo, Attribute attribute, List<Attribute> memberAttributes )
	{
		this.MemberInfo = memberInfo;
		this.Attribute = attribute;
		this.MemberAttributes = memberAttributes;
	}
}
