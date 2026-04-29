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
using System.Collections.Generic;
using System.Linq;
using Origam.DA;
using Origam.DA.Service;

namespace Origam.Workbench.Services.CoreServices;

public static class DataServiceFactory
{
    private static readonly IDictionary<string, IDataService> _dataServiceDictionary =
        new Dictionary<string, IDataService>();

    public static IDataService GetDataService()
    {
        return GetDataService(deployPlatform: null);
    }

    public static IDataService GetDataService(Platform deployPlatform)
    {
        string dictionarykey = "primary";
        if (deployPlatform != null && !deployPlatform.IsPrimary)
        {
            dictionarykey = deployPlatform.Name;
        }
        KeyValuePair<string, IDataService> keyValue = _dataServiceDictionary.FirstOrDefault(
            predicate: dict => dict.Key == dictionarykey
        );
        if (string.IsNullOrEmpty(value: keyValue.Key))
        {
            _dataServiceDictionary.Add(
                key: dictionarykey,
                value: CreateDataService(deployPlatform: deployPlatform)
            );
        }
        return _dataServiceDictionary.First(predicate: dict => dict.Key == dictionarykey).Value;
    }

    private static IDataService CreateDataService(Platform deployPlatform)
    {
        IDataService dataService = null;
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (settings == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorSettingsNotFound")
            );
        }
        string assembly = settings.DataDataService.Split(separator: ",".ToCharArray())[0].Trim();
        string classname = settings.DataDataService.Split(separator: ",".ToCharArray())[1].Trim();

        if (deployPlatform != null)
        {
            assembly = deployPlatform.DataService.Split(separator: ",".ToCharArray())[0].Trim();
            classname = deployPlatform.DataService.Split(separator: ",".ToCharArray())[1].Trim();
        }
        dataService =
            Reflector.InvokeObject(classname: assembly, assembly: classname) as IDataService;
        dataService.ConnectionString = settings.DataConnectionString;
        dataService.BulkInsertThreshold = settings.DataBulkInsertThreshold;
        dataService.UpdateBatchSize = settings.DataUpdateBatchSize;
        dataService.StateMachine =
            ServiceManager.Services.GetService(serviceType: typeof(IStateMachineService))
            as IStateMachineService;
        dataService.AttachmentService =
            ServiceManager.Services.GetService(serviceType: typeof(IAttachmentService))
            as IAttachmentService;
        dataService.UserDefinedParameters = true;
        (dataService as AbstractDataService).PersistenceProvider = persistence.SchemaProvider;
        if (deployPlatform != null)
        {
            dataService.ConnectionString = deployPlatform.DataConnectionString;
        }
        return dataService;
    }

    public static void ClearDataService()
    {
        foreach (var keyValue in _dataServiceDictionary)
        {
            IDataService dataService = keyValue.Value;
            dataService?.Dispose();
            dataService = null;
        }
        _dataServiceDictionary.Clear();
    }
}
