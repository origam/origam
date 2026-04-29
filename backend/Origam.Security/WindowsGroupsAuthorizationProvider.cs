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
using System.Collections;
using System.Data;
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

    public WindowsGroupsAuthorizationProvider() { }

    private Hashtable GetCache()
    {
        string cacheName = "WindowsGroupsAuthorizationProviderCache";
        Hashtable context = OrigamUserContext.Context;
        if (!context.Contains(key: cacheName))
        {
            context.Add(key: cacheName, value: new Hashtable());
        }
        return (Hashtable)OrigamUserContext.Context[key: cacheName];
    }

    #region IAuthorizationProvider Members
    public bool Authorize(System.Security.Principal.IPrincipal principal, string context)
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (context == null)
        {
            return false;
        }

        if (context.Trim() == "")
        {
            return false;
        }

        if (context == "*")
        {
            return true;
        }

        string[] roles = context.Split(separator: ";".ToCharArray());
        foreach (string roleTest in roles)
        {
            string[] rolePart = roleTest.Split(separator: "|".ToCharArray());
            string appRole = rolePart[0];
            bool negation = false;

            if (appRole.StartsWith(value: "!"))
            {
                appRole = appRole.Substring(startIndex: 1);
                negation = true;
            }
            if (appRole == "*")
            {
                return false;
            }

            foreach (Credential c in CredentialList(applicationRole: appRole))
            {
                bool process = true;
                bool result = false;
                if (rolePart.Length == 2 && rolePart[1] == "isReadOnly()")
                {
                    process = c.IsReadOnly;
                }
                if (process)
                {
                    if (principal.IsInRole(role: settings.SecurityDomain + "\\" + c.RoleName))
                    {
                        result = true;
                    }
                }
                if (result)
                {
                    if (negation)
                    {
                        return false;
                    }
                    return result;
                }
            }
            if (negation)
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
        if (cache.Contains(key: applicationRole))
        {
            return (Credential[])cache[key: applicationRole];
        }
        IServiceAgent dataServiceAgent;
        try
        {
            dataServiceAgent = (
                ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
                as IBusinessServicesService
            ).GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        }
        catch
        {
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorNoDataServiceAgent"));
        }
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: new Guid(g: "0f182ce7-5c13-497b-85b4-4f6366bd02ae"),
            methodId: new Guid(g: "c70f949d-d970-48aa-9d4b-f8f109e4ea5f")
        );
        query.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "OrigamApplicationRole_parName",
                value: applicationRole
            )
        );

        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        try
        {
            dataServiceAgent.Run();
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorWhenLoadingRoleList"),
                innerException: ex
            );
        }
        DataSet result = (DataSet)dataServiceAgent.Result;
        DataTable table = result.Tables[name: "OrigamRole"];
        Credential[] array = new Credential[table.Rows.Count];

        for (int i = 0; i < array.Length; i++)
        {
            DataRow row = table.Rows[index: i];
            string name = (string)row[columnName: "Name"];
            string alias = null;
            if (!row.IsNull(columnName: "Alias"))
            {
                alias = (string)table.Rows[index: i][columnName: "Alias"];
            }
            bool isReadOnly = false;
            if (
                table.ChildRelations.Contains(name: "OrigamRoleOrigamApplicationRole")
                && table
                    .ChildRelations[name: "OrigamRoleOrigamApplicationRole"]
                    .ChildTable.Columns.Contains(name: "IsFormReadOnly")
            )
            {
                DataRow[] assignments = row.GetChildRows(
                    relationName: "OrigamRoleOrigamApplicationRole"
                );
                if (assignments.Length > 0)
                {
                    isReadOnly = (bool)assignments[0][columnName: "IsFormReadOnly"];
                }
            }
            array[i] = new Credential(
                roleName: alias == null ? name : alias,
                isReadOnly: isReadOnly
            );
        }
        cache[key: applicationRole] = array;
        return array;
    }
    #endregion
}
