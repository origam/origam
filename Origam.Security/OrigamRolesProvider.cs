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

using System.Security.Principal;

using Microsoft.Practices.EnterpriseLibrary.Security;
using Microsoft.Practices.EnterpriseLibrary.Configuration;

namespace Origam.Security
{
	/// <summary>
	/// Summary description for OrigamRolesProvider.
	/// </summary>
	public class OrigamRolesProvider :  ConfigurationProvider, IRolesProvider
	{
		public OrigamRolesProvider()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public override void Initialize(ConfigurationView configurationView)
		{
		}

 
		#region IRolesProvider Members

		public IPrincipal GetRoles(IIdentity identity)
		{
			
			// TODO:  Add OrigamRolesProvider.GetRoles implementation
			return null;
		}

		#endregion
	}
}
