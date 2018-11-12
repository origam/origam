using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.Schema.EntityModel;
using Origam.ServerCore.Models;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Controllers
{
    public class Test
    {
        public int Id { get; set; }
    }

    public class DataController : AbstractController
    {
        public DataController(ILogger<MetaDataController> log) : base(log)
        {
        }

        [HttpPost("[action]")]
        public IEnumerable<object[]> Test([FromBody] Test test)
        {
            return null;
        }

        [HttpPost("[action]")]
        public IEnumerable<object[]> GetEntityData([FromBody] EntityData entityData)
        {
            var entity = (DataStructureEntity)ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(DataStructureEntity), new Key(entityData.DataStructureEntityId));

            var dataStructureId = Guid.Parse(entity.ParentNode.NodeId);

            DataStructureQuery query = new DataStructureQuery(
                dataStructureId,
                entity.Name,
                entityData.Filter,
                entityData.OrderingAsTuples,
                entityData.RowLimit);
            query.ColumnName = string.Join(";", entityData.ColumnNames);

            IDataService dataService = DataService.GetDataService();
            using (IDataReader reader = dataService.ExecuteDataReader(
                query, SecurityManager.CurrentPrincipal, null))
            {
                while (reader.Read())
                {
                    object[] values = new object[entityData.ColumnNames.Count];
                    reader.GetValues(values);
                    yield return values;
                }
            }
        }

    }
}
