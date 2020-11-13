using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.ServerCore.Model.HashTag;
using Origam.ServerCore.Resources;
using Origam.Services;
using Origam.Workbench.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace Origam.ServerCore.Controller
{
    [ApiController]
    [Route("internalApi/[controller]")]
    public class HashTagController : AbstractController
    {
        private readonly IStringLocalizer<SharedResources> localizer;

        public HashTagController(
            SessionObjects sessionObjects,
            IStringLocalizer<SharedResources> localizer,
            ILogger<AbstractController> log) : base(log, sessionObjects)
        {
            this.localizer = localizer;
        }

        [HttpGet("categories")]
        public IActionResult GetCategoriesRequest()
        {
            return RunWithErrorHandler(() =>
                Ok(GetCategories()));
        }

        [HttpGet("{categoryId}/objects")]
        public IActionResult GetObjectsRequest(string categoryId, [FromQuery] int limit,
            [FromQuery] int offset,
            [FromQuery] string searchPhrase)
        {
            return RunWithErrorHandler(() =>
                Ok(GetObjets(categoryId,limit,offset,searchPhrase)));
        }

        private object GetObjets(string categoryId, int limit, int offset, string searchPhrase)
        {
            IPersistenceService persistence = ServiceManager.Services.GetService(
                   typeof(IPersistenceService)) as IPersistenceService;
            var hashTag = persistence.SchemaProvider.RetrieveListByCategory<HashTag>(HashTag.CategoryConst)
                .Where(hashtag => hashtag.Name == categoryId);
            if(hashTag.Any())
            {
                HashTag hashT = hashTag.First();
                return GetLookupData(hashT,limit,offset,searchPhrase);
            }
            return new Exception("No Data");
        }

        private object GetLookupData(HashTag hashT, int limit, int offset, string searchPhrase)
        {
            IDataLookupService lookupService
                    = ServiceManager.Services.GetService<IDataLookupService>();
            var internalRequest = new LookupListRequest
            {
                LookupId = hashT.LookupId,
                FieldName = hashT.Lookup.ValueValueMember,
                CurrentRow = null,
                ShowUniqueValues = false,
                SearchText = "%" + searchPhrase + "%",
                PageSize = limit,
                PageNumber = offset,
                ParameterMappings = null
            };
            var dataTable = lookupService.GetList(internalRequest);
            return AreColumnNamesValid(hashT.Lookup.ValueDisplayMember.Split(";"), dataTable)
                ? Result.Success<IEnumerable<object[]>, IActionResult>(
                    GetRowData(internalRequest, dataTable,hashT.Lookup.ValueDisplayMember.Split(";")))
                : Result.Failure<IEnumerable<object[]>, IActionResult>(
                    BadRequest("Some of the supplied column names are not in the table."));
        }

        private IEnumerable<object[]> GetRowData(
            LookupListRequest input, DataTable dataTable, string[] columnNames)
        {
            var lookup = FindItem<DataServiceDataLookup>(input.LookupId).Value;
            if (lookup.IsFilteredServerside || string.IsNullOrEmpty(input.SearchText))
            {
                return dataTable.Rows
                    .Cast<DataRow>()
                    .Select(row => GetColumnValues(row, columnNames));
            }
            var columnNamesWithoutPrimaryKey = FilterOutPrimaryKey(
                columnNames, dataTable.PrimaryKey);
            var result =  dataTable.Rows
                .Cast<DataRow>()
                .Where(row => Filter(row, columnNamesWithoutPrimaryKey,
                    input.SearchText))
                .Select(row => GetColumnValues(row, columnNames));
            return result;
        }

        private static object[] GetColumnValues(
            DataRow row, IEnumerable<string> columnNames)
        {
            return columnNames.Select(colName => row[colName]).ToArray();
        }
        private static string[] FilterOutPrimaryKey(
            IEnumerable<string> columnNames, DataColumn[] primaryKey)
        {
            return columnNames
                .Where(columnName => primaryKey.All(
                    dataColumn => dataColumn.ColumnName != columnName))
                .ToArray();
        }
        private static bool Filter(
            DataRow row, IEnumerable<string> columnNames, string likeParameter)
        {
            string search = likeParameter.Replace("%", "");
            return columnNames
                .Select(colName => row[colName])
                .Any(colValue =>
                    colValue.ToString().Contains(
                        search,
                        StringComparison.InvariantCultureIgnoreCase));
        }
        private static bool AreColumnNamesValid(
           string[] columnNames, DataTable dataTable)
        {
            var actualColumnNames = dataTable.Columns
                .Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToArray();
            return columnNames
                .All(colName => actualColumnNames.Contains(colName));
        }

        private List<HashTagCategory> GetCategories()
        {
            List<HashTagCategory> hashtagCategoryList = new List<HashTagCategory>();
            IPersistenceService persistence = ServiceManager.Services.GetService(
                   typeof(IPersistenceService)) as IPersistenceService;
            List<HashTag> listOfHastags = persistence.SchemaProvider.RetrieveListByCategory<HashTag>(HashTag.CategoryConst);
            foreach (HashTag hashTag in listOfHastags)
            {
                if (TestRole(hashTag.Roles))
                {
                    HashTagCategory hashtagCategory = new HashTagCategory
                    {
                        hashtagName = hashTag.Name,
                        hashtagLabel = hashTag.Label,
                        objectComboboxMetada = CreateComboBox(hashTag)
                    };
                    hashtagCategoryList.Add(hashtagCategory);
                }
            }
            return hashtagCategoryList;
        }

        private bool TestRole(string roles)
        {
            if (!SecurityManager.GetAuthorizationProvider().Authorize(SecurityManager.CurrentPrincipal, roles))
            {
               return false;
            }
            return true;
        }

        private string CreateComboBox(HashTag hasTag)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement controlElement = doc.CreateElement("control");
            doc.AppendChild(controlElement);
            ComboBoxBuilder.Build(controlElement, hasTag.LookupId, false, null, null);
            return doc.InnerXml;
        }
    }
}
