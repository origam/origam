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
using Origam.Extensions;
using Origam.Workbench.Services;

namespace Origam.Security;

/// <summary>
/// Summary description for OrigamProfileProvider.
/// </summary>
public class OrigamProfileProvider : AbstractProfileProvider
{
    private const string PROFILE_DATA_STRUCTURE = "8e628d99-986a-4b46-9e78-01a2a91ee85a";
    private const string NEW_PROFILE_DATA_STRUCTURE = "37aa9baa-ac4d-4252-8450-4034c1c36e3e";
    private const string PROFILE_METHOD = "0ce6f260-6401-401a-a70c-7beaf8564075";

    #region IProfileProvider Members
    override public void AddUser(string name, string userName)
    {
        // load existing
        DataSet data = GetProfileData(userName: userName);
        DataTable table = data.Tables[name: "BusinessPartner"];
        DataRow row = table.NewRow();
        row[columnName: "Id"] = Guid.NewGuid();
        row[columnName: "Name"] = name;
        row[columnName: "UserName"] = userName;
        row[columnName: "RecordCreated"] = DateTime.Now;
        table.Rows.Add(row: row);
        // store
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: new Guid(g: NEW_PROFILE_DATA_STRUCTURE)
        );
        // if data service would try to get identity,
        // we would get into recursion
        query.LoadByIdentity = false;
        query.FireStateMachineEvents = false;
        IServiceAgent dataServiceAgent = GetAgent();
        dataServiceAgent.MethodName = "StoreDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Parameters.Add(key: "Data", value: data);
        dataServiceAgent.Run();
    }

    public override object GetProfile(string userName)
    {
        Hashtable profileCacheByIdentity = GetCacheByName();
        if (profileCacheByIdentity.Contains(key: userName))
        {
            return profileCacheByIdentity[key: userName];
        }

        try
        {
            UserProfile profile = new UserProfile();
            lock (profileCacheByIdentity)
            {
                DataSet result = GetProfileData(userName: userName);
                if (result.Tables[name: "BusinessPartner"].Rows.Count == 0)
                {
                    throw new ProfileNotFoundException(
                        message: ResourceUtils.GetString(
                            key: "ErrorProfileUnavailable",
                            args: userName
                        )
                    );
                }
                DataRow row = result.Tables[name: "BusinessPartner"].Rows[index: 0];
                profile.Id = (Guid)row[columnName: "Id"];
                profile.FullName = (string)row[columnName: "FullName"];
                if (
                    row.Table.Columns.Contains(name: "Resource_Id")
                    && (!row.IsNull(columnName: "Resource_Id"))
                )
                {
                    profile.ResourceId = (Guid)row[columnName: "Resource_Id"];
                }
                if (
                    row.Table.Columns.Contains(name: "BusinessUnit_Id")
                    && (!row.IsNull(columnName: "BusinessUnit_Id"))
                )
                {
                    profile.BusinessUnitId = (Guid)row[columnName: "BusinessUnit_Id"];
                }
                if (
                    row.Table.Columns.Contains(name: "Organization_Id")
                    && (!row.IsNull(columnName: "Organization_Id"))
                )
                {
                    profile.OrganizationId = (Guid)row[columnName: "Organization_Id"];
                }
                if (
                    row.Table.Columns.Contains(name: "UserEmail")
                    && (!row.IsNull(columnName: "UserEmail"))
                )
                {
                    profile.Email = (string)row[columnName: "UserEmail"];
                }
                profileCacheByIdentity[key: userName] = profile;
            }
            return profile;
        }
        catch (ProfileNotFoundException ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(message: ex.Message, ex: ex);
            }
            throw;
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(message: ex.Message, ex: ex);
            }
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorUnableToLoadProfile0")
                    + Environment.NewLine
                    + Environment.NewLine
                    + ResourceUtils.GetString(key: "ErrorUnableToLoadProfile1")
                    + Environment.NewLine
                    + ResourceUtils.GetString(key: "ErrorUnableToLoadProfile2"),
                innerException: ex
            );
        }
    }

    private static DataSet GetProfileData(string userName)
    {
        DataStructureQuery query = GetProfileQuery();
        query.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "BusinessPartner_parUserName",
                value: userName
            )
        );
        IServiceAgent dataServiceAgent = GetAgent();
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Run();
        DataSet result = (DataSet)dataServiceAgent.Result;
        return result;
    }

    private static DataStructureQuery GetProfileQuery()
    {
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: new Guid(g: PROFILE_DATA_STRUCTURE),
            methodId: new Guid(g: PROFILE_METHOD)
        );
        // if data service would try to get identity,
        // we would get into recursion
        query.LoadByIdentity = false;
        return query;
    }
    #endregion
}
