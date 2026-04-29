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
using System.Security.Principal;
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Security;

/// <summary>
/// Summary description for OrigamDatabaseTenantAuthorizationProvider.
/// </summary>
public class OrigamDatabaseTenantAuthorizationProvider : IOrigamAuthorizationProvider
{
    public OrigamDatabaseTenantAuthorizationProvider() { }

    private Hashtable GetCache()
    {
        string cacheName = "DatabaseAuthorizationProviderCache";
        Hashtable context = OrigamUserContext.Context;
        if (!context.Contains(key: cacheName))
        {
            context.Add(key: cacheName, value: new Hashtable());
        }
        return (Hashtable)OrigamUserContext.Context[key: cacheName];
    }

    #region IAuthorizationProvider Members
    public bool Authorize(IPrincipal principal, string context)
    {
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
            string[] rolePart = roleTest.Split(
                separator: SecurityManager.ROLE_SUFFIX_DIVIDER.ToCharArray()
            );
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

            foreach (Credential c in RoleList(principal: principal))
            {
                bool process = true;
                if (rolePart.Length == 2 && rolePart[1] == SecurityManager.READ_ONLY_ROLE_SUFFIX)
                {
                    process = c.IsReadOnly;
                }
                else if (
                    rolePart.Length == 2
                    && rolePart[1] == SecurityManager.INITIAL_SCREEN_ROLE_SUFFIX
                )
                {
                    process = c.IsInitialScreen;
                }
                bool result = false;
                if (process)
                {
                    if (appRole.Equals(value: c.RoleName))
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
            // we must not force readonly if there is a negation in role list
            if (
                negation
                && !(rolePart.Length == 2 && rolePart[1] == SecurityManager.READ_ONLY_ROLE_SUFFIX)
            )
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    #region private methods
    Credential[] RoleList(IPrincipal principal)
    {
        Hashtable cache = GetCache();
        string name = principal.Identity.Name;
        if (!principal.Identity.IsAuthenticated)
        {
            name = "guest";
        }
        if (cache.Contains(key: name))
        {
            return (Credential[])cache[key: name];
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
        UserProfile profile =
            SecurityManager.GetProfileProvider().GetProfile(identity: principal.Identity)
            as UserProfile;
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: new Guid(g: "24ec7286-942e-4ed8-9934-d7619f6216d7"),
            methodId: new Guid(g: "603938fd-a9dd-4807-aca2-a161bc6fadc5")
        );
        query.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "BusinessPartnerOrigamRole_parBusinessPartnerId",
                value: profile.Id
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
        DataTable table = result.Tables[name: "OrigamRoleOrigamApplicationRole"];
        if (table == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorRoleListNotLoaded")
            );
        }
        // retrieve application roles for organization
        DataStructureQuery query2 = new DataStructureQuery(
            dataStructureId: new Guid(g: "c68dca4b-5690-40f5-8b11-4aa6efdc1b04"),
            methodId: new Guid(g: "d4452a51-54d8-46ba-81cf-8af26a733534")
        );
        query2.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "OrganizationBusinessPartner_parUserBusinessPartnerId",
                value: profile.Id
            )
        );
        query2.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "OrganizationBusinessPartner_parOrganizationId",
                value: profile.OrganizationId
            )
        );

        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query2);
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
        DataSet result2 = (DataSet)dataServiceAgent.Result;
        DataTable table2 = result2.Tables[name: "OrganizationOrigamRoleOrigamApplicationRole"];
        if (table2 == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorOrganizationRoleListNotLoaded")
            );
        }

        Credential[] array = new Credential[table.Rows.Count + table2.Rows.Count];

        for (int i = 0; i < table.Rows.Count; i++)
        {
            DataRow row = table.Rows[index: i];
            array[i] = new Credential(
                RoleName: (string)row[columnName: "OrigamApplicationRole_Name"],
                IsReadOnly: table.Columns.Contains(name: "IsFormReadOnly")
                    && (bool)row[columnName: "IsFormReadOnly"],
                IsInitialScreen: table.Columns.Contains(name: "IsInitialScreen")
                    && (bool)row[columnName: "IsInitialScreen"]
            );
        }
        for (int i = table.Rows.Count; i < array.Length; i++)
        {
            DataRow row = table2.Rows[index: i - table.Rows.Count];

            array[i] = new Credential(
                RoleName: (string)row[columnName: "OrigamApplicationRole_Name"],
                IsReadOnly: table.Columns.Contains(name: "IsFormReadOnly")
                    && (bool)row[columnName: "IsFormReadOnly"],
                IsInitialScreen: table.Columns.Contains(name: "IsInitialScreen")
                    && (bool)row[columnName: "IsInitialScreen"]
            );
        }
        cache[key: name] = array;
        return array;
    }
    #endregion
}
