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
using System.Security.Principal;
using Origam.DA;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Security;

public abstract class AbstractProfileProvider : IOrigamProfileProvider
{
    protected ISchemaService _schemaService =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;
    protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    #region IProfileProvider Members
    public object GetProfile(Guid profileId)
    {
        Hashtable profileCacheById = GetCacheById();
        if (profileCacheById.Contains(key: profileId))
        {
            return profileCacheById[key: profileId];
        }
        UserProfile profile = new UserProfile();
        profile.Id = profileId;
        lock (profileCacheById)
        {
            DataStructureQuery query = new DataStructureQuery(
                dataStructureId: new Guid(g: "1a90ab22-6bc8-416c-92ee-e053272f225c"),
                methodId: new Guid(g: "ef310516-e1e2-4a01-b0b2-b259ad14e1b5")
            );
            query.Parameters.Add(
                value: new QueryParameter(_parameterName: "BusinessPartner_parId", value: profileId)
            );
            IServiceAgent dataServiceAgent = GetAgent();
            dataServiceAgent.MethodName = "GetScalarValueByQuery";
            dataServiceAgent.Parameters.Clear();
            dataServiceAgent.Parameters.Add(key: "Query", value: query);
            dataServiceAgent.Parameters.Add(key: "ColumnName", value: "LookupText");
            dataServiceAgent.Run();
            object result = dataServiceAgent.Result;
            if (result == null)
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "ErrorProfileUnavailable",
                        args: profileId.ToString()
                    )
                );
            }
            profile.FullName = (string)result;
            profileCacheById[key: profileId] = profile;
        }

        return profile;
    }

    public abstract object GetProfile(string userName);
    public abstract void AddUser(string name, string userName);

    public object GetProfile(IIdentity identity)
    {
        string name = identity.Name;
        if (!identity.IsAuthenticated)
        {
            name = "guest";
        }
        return GetProfile(userName: name);
    }

    public void SetProfile(IIdentity identity, object profile)
    {
        throw new NotImplementedException(
            message: "AbstractProfileProvider.SetProfile not implemented"
        );
    }
    #endregion
    protected static IServiceAgent GetAgent()
    {
        IBusinessServicesService bus =
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService;
        if (bus == null)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorModelNotLoaded")
            );
        }

        return bus.GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
    }

    protected Hashtable GetCacheById()
    {
        string cacheName = "ProfileCacheById";
        Hashtable context = OrigamUserContext.Context;
        if (!context.Contains(key: cacheName))
        {
            context.Add(key: cacheName, value: new Hashtable());
        }
        return (Hashtable)OrigamUserContext.Context[key: cacheName];
    }

    protected Hashtable GetCacheByName()
    {
        return OrigamUserContext.GetContextItem(cacheName: "ProfileCacheByName");
    }
}
