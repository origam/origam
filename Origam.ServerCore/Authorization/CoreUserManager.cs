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
                    new ModelElementKey(ModelItems.ORIGAM_USER_DATA_STRUCTURE));
            DataSet origamUserDataSet = dataSetGenerator.CreateDataSet(
                dataStructure);
            DataRow origamUserRow
                = origamUserDataSet.Tables["OrigamUser"].NewRow();
            UserTools.AddToOrigamUserRow(user ,origamUserRow);
            origamUserDataSet.Tables["OrigamUser"].Rows.Add(origamUserRow);

            DataService.StoreData(ModelItems.ORIGAM_USER_DATA_STRUCTURE,
                origamUserDataSet, false, user.TransactionId);
        }
        
        public void SetInitialSetupComplete()
        {
            IParameterService parameterService =
                ServiceManager.Services.GetService(typeof(IParameterService)) as
                    IParameterService;
            parameterService.SetCustomParameterValue(INITIAL_SETUP_PARAMETERNAME, true,
                Guid.Empty, 0, null, true, 0, 0, null);
        }
          public bool IsInitialSetupNeeded()
        {
            IParameterService parameterService =
                ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
        
            return !(bool)parameterService.GetParameterValue(INITIAL_SETUP_PARAMETERNAME);
        }
    }
}