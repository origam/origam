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

namespace Origam;

/// <summary>
/// Summary description for UserProfile.
/// </summary>
public class UserProfile
{
	public UserProfile()
	{
		}

	private Guid _id;
	public Guid Id
	{
		get
		{
				return _id;
			}
		set
		{
				_id = value;
			}
	}

	private Guid _resourceId;
	public Guid ResourceId
	{
		get
		{
				return _resourceId;
			}
		set
		{
				_resourceId = value;
			}
	}

	private Guid _businessUnitId;
	public Guid BusinessUnitId
	{
		get
		{
				return _businessUnitId;
			}
		set
		{
				_businessUnitId = value;
			}
	}

	private Guid _organizationId;
	public Guid OrganizationId
	{
		get
		{
				return _organizationId;
			}
		set
		{
				_organizationId = value;
			}
	}

	private string _fullName;
	public string FullName
	{
		get
		{
				return _fullName;
			}
		set
		{
				_fullName = value;
			}
	}

	private string _email;
	public string Email
	{
		get
		{
                return _email;
            }
		set
		{
                _email = value;
            }
	}
}