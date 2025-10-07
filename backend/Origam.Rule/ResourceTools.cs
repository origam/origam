#region license
/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Rule;

public interface IResourceTools
{
    string ResourceIdByActiveProfile();
}

public class ResourceTools : IResourceTools
{
    private readonly IBusinessServicesService businessService;
    private readonly Func<UserProfile> userProfileGetter;

    public ResourceTools(
        IBusinessServicesService businessService,
        Func<UserProfile> userProfileGetter
    )
    {
        this.businessService = businessService;
        this.userProfileGetter = userProfileGetter;
    }

    public string ResourceIdByActiveProfile()
    {
        DataStructureQuery query = new DataStructureQuery(
            new Guid("d0d0d847-36dc-4987-95e5-43c4d8d0d78f"),
            new Guid("84848e4c-129c-4079-95a4-6319e21399af")
        );

        query.Parameters.Add(
            new QueryParameter("Resource_parBusinessPartnerId", userProfileGetter().Id)
        );

        DataSet ds = LoadData(query);

        if (ds.Tables["Resource"].Rows.Count == 0)
        {
            throw new Exception(ResourceUtils.GetString("ErrorNoResource"));
        }
        else if (ds.Tables["Resource"].Rows.Count > 1)
        {
            throw new Exception(
                ResourceUtils.GetString("ErrorMoreResources", userProfileGetter().Id)
            );
        }
        else
        {
            return ds.Tables["Resource"].Rows[0]["Id"].ToString();
        }
    }

    private DataSet LoadData(DataStructureQuery query)
    {
        var dataServiceAgent = businessService.GetAgent("DataService", null, null);
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add("Query", query);

        dataServiceAgent.Run();

        return dataServiceAgent.Result as DataSet;
    }
}
