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
using System.Data;
using System.Collections;
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Security;
/// <summary>
/// Summary description for WindowsGroupsAuthorizationProvider.
/// </summary>
public class WindowsGroupsAuthorizationProvider : IOrigamAuthorizationProvider
{
	private class Credential
	{
		public string RoleName;
		public bool IsReadOnly;
		public Credential(string roleName, bool isReadOnly)
		{
			RoleName = roleName;
			IsReadOnly = isReadOnly;
		}
	}
	public WindowsGroupsAuthorizationProvider()
	{
	}
	private Hashtable GetCache()
	{
		string cacheName = "WindowsGroupsAuthorizationProviderCache";
		Hashtable context = OrigamUserContext.Context;
		if(! context.Contains(cacheName))
		{
			context.Add(cacheName, new Hashtable());
		}
		return (Hashtable)OrigamUserContext.Context[cacheName];
	}
	#region IAuthorizationProvider Members
	public bool Authorize(System.Security.Principal.IPrincipal principal, string context)
	{
		OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
		
		//			System.Windows.Forms.MessageBox.Show("Authorizing identity: '" + principal.Identity.Name + "'");
		if(context == null) return false;
		if(context.Trim() == "") return false;
		if(context == "*") return true;
		string[] roles = context.Split(";".ToCharArray());
//			string[] winRoles = GetWindowsIdentityRoles(principal.Identity as WindowsIdentity);
//			DirectoryEntry entry = new DirectoryEntry("WinNT://" + principal.Identity.Name.Replace("\\", "/"));
//			//System.DirectoryServices.DirectoryEntry entry = new DirectoryEntry("WinNT://AS/tvavrda");
//
//			object groups = entry.Invoke("Groups");
//
//			foreach (object ob in (IEnumerable)groups)
//			{
//				// Create object for each group.
//				DirectoryEntry group = new DirectoryEntry(ob);
//
//				foreach(string role in roles)
//				{
////					System.Windows.Forms.MessageBox.Show("Comparing role '" + role + "' with group '" + group.Name + "'");
//					if(group.Name.Trim().ToUpper().CompareTo(role.Trim().ToUpper()) == 0)
//					{
////						System.Windows.Forms.MessageBox.Show("Success");
//						return true;
//					}
//				}
//			}
		foreach(string roleTest in roles)
		{
			string[] rolePart = roleTest.Split("|".ToCharArray());
			string appRole = rolePart[0];
			bool negation = false;
			
			if(appRole.StartsWith("!"))
			{
				appRole = appRole.Substring(1);
				negation = true;
			}
			if(appRole == "*") return false;
			foreach(Credential c in CredentialList(appRole))
			{
				bool process = true;
				bool result = false;
				if(rolePart.Length == 2 && rolePart[1] == "isReadOnly()")
				{
					process = c.IsReadOnly;
				}
				if(process)
				{
					if(principal.IsInRole(settings.SecurityDomain + "\\" + c.RoleName))
					{
						result = true;
					}
				}
				if(result)
				{
					if(negation)
					{
						return false;
					}
					return result;
				}
			}
			if(negation)
			{
				return true;
			}
		}
		return false;
	}
	#endregion
	#region private methods
	Credential[] CredentialList(string applicationRole)
	{
		Hashtable cache = GetCache();
		if(cache.Contains(applicationRole))
		{
			return (Credential[])cache[applicationRole];
		}
		IServiceAgent dataServiceAgent;
		try
		{
			dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
		}
		catch
		{
			throw new Exception(ResourceUtils.GetString("ErrorNoDataServiceAgent"));
		}
		DataStructureQuery query = new DataStructureQuery(new Guid("0f182ce7-5c13-497b-85b4-4f6366bd02ae"), new Guid("c70f949d-d970-48aa-9d4b-f8f109e4ea5f"));
		query.Parameters.Add(new QueryParameter("OrigamApplicationRole_parName", applicationRole));
			
		dataServiceAgent.MethodName = "LoadDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", query);
		try
		{
			dataServiceAgent.Run();
		}
		catch(Exception ex)
		{
			throw new Exception(ResourceUtils.GetString("ErrorWhenLoadingRoleList"), ex);
		}
		DataSet result = (DataSet)dataServiceAgent.Result;
		DataTable table = result.Tables["OrigamRole"];
		Credential[] array = new Credential[table.Rows.Count];
		
		for(int i = 0; i < array.Length; i++)
		{
			DataRow row = table.Rows[i];
			string name = (string)row["Name"];
			string alias = null;
			if(!row.IsNull("Alias"))
			{
				alias = (string)table.Rows[i]["Alias"];
			}
			bool isReadOnly = false;
			if(table.ChildRelations.Contains("OrigamRoleOrigamApplicationRole") && table.ChildRelations["OrigamRoleOrigamApplicationRole"].ChildTable.Columns.Contains("IsFormReadOnly"))
			{
				DataRow[] assignments = row.GetChildRows("OrigamRoleOrigamApplicationRole");
				if(assignments.Length > 0)
				{
					isReadOnly = (bool)assignments[0]["IsFormReadOnly"];
				}
			}
			array[i] = new Credential(alias == null ? name : alias, isReadOnly);
		}
		cache[applicationRole] = array;
		return array;
	}
	#endregion
}
