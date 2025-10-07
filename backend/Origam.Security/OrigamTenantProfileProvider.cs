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

public class OrigamTenantProfileProvider : AbstractProfileProvider
{
    static readonly Guid CURRENT_ORGANIZATION_BUSINESS_PARTNER_ID_GUID = new Guid(
        "24d35dbd-f113-4925-99d4-d3136aa43ae6"
    );
    static readonly Guid CURRENT_ORGANIZATION_BUSINESS_PARTNER_ID__NOT_SET_YET = new Guid(
        "79fe3ab3-4313-4e22-80f0-679c94322b84"
    );

    #region IProfileProvider Members
    public override void AddUser(string name, string userName)
    {
        throw new NotImplementedException();
    }

    public override object GetProfile(string userName)
    {
        Hashtable profileCacheByIdentity = GetCacheByName();
        if (profileCacheByIdentity.Contains(userName))
        {
            return profileCacheByIdentity[userName];
        }
        else
        {
            try
            {
                UserProfile profile = new UserProfile();
                lock (profileCacheByIdentity)
                {
                    DataStructureQuery query = new DataStructureQuery(
                        new Guid("c1b5852d-9c79-408f-84d1-71cf5f8a899a"),
                        new Guid("42160561-5c04-4a97-b836-53e2d6fe9d97"),
                        Guid.Empty,
                        new Guid("fbbcda00-5df4-425c-8d2e-d3b6ae73caa0")
                    );
                    query.Parameters.Add(
                        new QueryParameter("BusinessPartner_parUserName", userName)
                    );
                    // if data service would try to get identity,
                    //we would get into recursion
                    query.LoadByIdentity = false;
                    IServiceAgent dataServiceAgent = GetAgent();
                    dataServiceAgent.MethodName = "LoadDataByQuery";
                    dataServiceAgent.Parameters.Clear();
                    dataServiceAgent.Parameters.Add("Query", query);
                    dataServiceAgent.Run();
                    DataSet result = (DataSet)dataServiceAgent.Result;
                    if (result.Tables["BusinessPartner"].Rows.Count == 0)
                    {
                        throw new ProfileNotFoundException(
                            ResourceUtils.GetString("ErrorProfileUnavailable", userName)
                        );
                    }
                    DataRow row = result.Tables["BusinessPartner"].Rows[0];
                    profile.Id = (Guid)row["Id"];
                    // find out about current switch obp
                    IParameterService _parameterService =
                        ServiceManager.Services.GetService(typeof(IParameterService))
                        as IParameterService;
                    Guid? switchBusinessPartnerId =
                        _parameterService.GetParameterValue(
                            CURRENT_ORGANIZATION_BUSINESS_PARTNER_ID_GUID,
                            profile.Id
                        ) as Guid?;
                    DataRow obpRow = null;
                    if (
                        result.Tables["OrganizationBusinessPartner"].Rows.Count > 0
                        && CURRENT_ORGANIZATION_BUSINESS_PARTNER_ID__NOT_SET_YET.Equals(
                            switchBusinessPartnerId
                        )
                    )
                    {
                        obpRow = result.Tables["OrganizationBusinessPartner"].Rows[0];
                    }
                    // switched to some particular organization business partner?
                    if (
                        switchBusinessPartnerId != null
                        && !Guid.Empty.Equals(switchBusinessPartnerId.Value)
                        && !CURRENT_ORGANIZATION_BUSINESS_PARTNER_ID__NOT_SET_YET.Equals(
                            switchBusinessPartnerId
                        )
                    )
                    {
                        foreach (
                            DataRow iObpRow in result.Tables["OrganizationBusinessPartner"].Rows
                        )
                        {
                            if (switchBusinessPartnerId.Equals((Guid)iObpRow["Id"]))
                            {
                                // we found the organization business partner
                                // switch must be valid - must not to other's obp profile
                                if ((Guid)iObpRow["refSwitchUserBusinessPartnerId"] == profile.Id)
                                {
                                    obpRow = iObpRow;
                                    break;
                                }
                            }
                        }
                    }
                    // set organization specific parameters
                    if (obpRow != null)
                    {
                        // we switch to a particular organization
                        // profile.FullName = (string)obpRow["SwitchFullName"];
                        profile.OrganizationId = (Guid)obpRow["refSwitchOrganizationId"];
                    }

                    profile.FullName = (string)row["FullName"] + " (" + userName + ")";

                    if (row.Table.Columns.Contains("Resource_Id") && (!row.IsNull("Resource_Id")))
                    {
                        profile.ResourceId = (Guid)row["Resource_Id"];
                    }
                    if (
                        row.Table.Columns.Contains("BusinessUnit_Id")
                        && (!row.IsNull("BusinessUnit_Id"))
                    )
                    {
                        profile.BusinessUnitId = (Guid)row["BusinessUnit_Id"];
                    }
                    profileCacheByIdentity[userName] = profile;
                }
                return profile;
            }
            catch (ProfileNotFoundException ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.LogOrigamError(ex.Message, ex);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.LogOrigamError(ex.Message, ex);
                }
                throw new Exception(
                    ResourceUtils.GetString("ErrorUnableToLoadProfile0")
                        + Environment.NewLine
                        + Environment.NewLine
                        + ResourceUtils.GetString("ErrorUnableToLoadProfile1")
                        + Environment.NewLine
                        + ResourceUtils.GetString("ErrorUnableToLoadProfile2"),
                    ex
                );
            }
        }
    }
    #endregion
}
