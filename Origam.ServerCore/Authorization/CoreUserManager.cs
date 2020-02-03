#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Security.Common;
using Origam.Security.Identity;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Authorization
{
    public class CoreUserManager: UserManager<IOrigamUser>
    {
        public static readonly Guid ORIGAM_USER_DATA_STRUCTURE
            = new Guid("43b67a51-68f3-4696-b08d-de46ae0223ce");
        private const string INITIAL_SETUP_PARAMETERNAME = "InitialUserCreated";
        
        public CoreUserManager(IUserStore<IOrigamUser> store, 
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<IOrigamUser> passwordHasher, 
            IEnumerable<IUserValidator<IOrigamUser>> userValidators, 
            IEnumerable<IPasswordValidator<IOrigamUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<IOrigamUser>> logger) 
            : base(store, optionsAccessor, passwordHasher, 
                userValidators, passwordValidators, keyNormalizer,
                errors, services, logger)
        {
        }

        public void CreateOrigamUser(IOrigamUser user)
        {
            DatasetGenerator dataSetGenerator = new DatasetGenerator(true);
            IPersistenceService persistenceService = ServiceManager.Services
                .GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataStructure dataStructure = (DataStructure) persistenceService
                .SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem),
                    new ModelElementKey(ORIGAM_USER_DATA_STRUCTURE));
            DataSet origamUserDataSet = dataSetGenerator.CreateDataSet(
                dataStructure);
            DataRow origamUserRow
                = origamUserDataSet.Tables["OrigamUser"].NewRow();
            UserTools.AddToOrigamUserRow(user ,origamUserRow);
            origamUserDataSet.Tables["OrigamUser"].Rows.Add(origamUserRow);

            DataService.StoreData(ORIGAM_USER_DATA_STRUCTURE,
                origamUserDataSet, false, user.TransactionId);
        }
        
        public void SetInitialSetupComplete()
        {
            ServiceManager.Services
                .GetService<IParameterService>()
                .SetCustomParameterValue(INITIAL_SETUP_PARAMETERNAME, true,
                Guid.Empty, 0, null, true, 0, 0, null);
        }
          public bool IsInitialSetupNeeded()
          {
              return !(bool)ServiceManager.Services
                .GetService<IParameterService>()
                .GetParameterValue(INITIAL_SETUP_PARAMETERNAME);
        }
    }
}