using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Security.Common;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Security.Identity
{
    public class CoreUserManager: UserManager<IOrigamUser>, IFrameworkSpecificManager
    {
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
            UserTools.AddToDataRow(user ,origamUserRow);
            origamUserDataSet.Tables["OrigamUser"].Rows.Add(origamUserRow);

            DataService.StoreData(ModelItems.ORIGAM_USER_DATA_STRUCTURE,
                origamUserDataSet, false, user.TransactionId);
        }

      

        HashSet<string> emailConfirmedUserIds= new HashSet<string>();

        public void SetEmailConfirmed(string userId)
        {
            emailConfirmedUserIds.Add(userId);
        }

        public bool EmailConfirmed(string id)
        {
            return emailConfirmedUserIds.Contains(id);
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public bool UserExists(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GeneratePasswordResetTokenAsync1(string toString)
        {
            throw new NotImplementedException();
        }

        public int? TokenLifespan { get; }


    }
}