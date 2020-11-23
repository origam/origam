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
        private readonly ILogger<AbstractController> logger;

        public HashTagController(
            SessionObjects sessionObjects,
            IStringLocalizer<SharedResources> localizer,
            ILogger<AbstractController> log) : base(log, sessionObjects)
        {
            this.logger = log;
        }

        [HttpGet("categories")]
        public IActionResult GetCategoriesRequest()
        {
            return RunWithErrorHandler(() =>
                Ok(GetCategories()));
        }

        [HttpGet("{categoryId}/objects")]
        public IActionResult GetObjectsRequest(string categoryId, [FromQuery] int limit,
            [FromQuery] int pageNumber,
            [FromQuery] string searchPhrase)
        {
            return RunWithErrorHandler(() =>
                GetObjets(categoryId,limit,pageNumber,searchPhrase));
        }

        [HttpPost("{categoryId}/labels")]
        public IActionResult GetLookupLabelRequest(string categoryId,
            [FromBody] HashtagLabelInput label)
        {
            return RunWithErrorHandler(() =>
                GetLookupLabel(categoryId,label.LabelIds));
        }

        private IActionResult GetLookupLabel(string categoryId, object[] labelIds)
        {
            HashtagCategory category = GetCategory(categoryId);
            if (TestRole(category.Roles))
            {
                var labelDictionary = GetLookupLabelsInternal(category, labelIds);
                return Ok(labelDictionary);
            }
            return Forbid();
        }

        private HashtagCategory GetCategory(string categoryId)
        {
            var hashtagProvider = GetHasTagProvider();
            HashtagCategory category = hashtagProvider.GetChildByName(
                categoryId, HashtagCategory.CategoryConst) as HashtagCategory;
            return category;
        }

        private Dictionary<object, string> GetLookupLabelsInternal(
           HashtagCategory input, object[] labelIds)
        {
            IDataLookupService lookupService
                   = ServiceManager.Services.GetService<IDataLookupService>();
            var labelDictionary = labelIds.ToDictionary(
                    id => id,
                    id =>
                    {
                        object lookupResult
                            = lookupService.GetDisplayText(
                                input.LookupId, id, false, true, null);
                        return lookupResult is decimal result
                            ? result.ToString("0.#")
                            : lookupResult.ToString();
                    });
            return labelDictionary;
        }

        [HttpPost("[action]")]
        public IActionResult GetMenuId([FromBody] GetHashtagMenuInput input)
        {
            return RunWithErrorHandler(() => Ok(GetMenuId(
                hashtagCategory: input.Category,
                ReferenceId: input.ReferenceId))
            );
        }

        private string GetMenuId(string hashtagCategory, Guid ReferenceId)
        {
            return ServiceManager.Services
                .GetService<IDataLookupService>()
                .GetMenuBinding(GetCategory(hashtagCategory).LookupId, ReferenceId)
                .MenuId;
        }

        private IActionResult GetObjets(string categoryId, int limit, int pageNumber, string searchPhrase)
        {
            HashtagCategory category = GetCategory(categoryId);
            if (category != null)
            {
                 var lookupData =  GetLookupData(category,limit,pageNumber,searchPhrase);
                return lookupData.IsSuccess ? Ok(lookupData.Value) : lookupData.Error;
            }
            return NotFound();
        }

        private HashTagSchemaItemProvider GetHasTagProvider()
        {
            var schemaservice = ServiceManager.Services.GetService<SchemaService>();
            return schemaservice.GetProvider<HashTagSchemaItemProvider>();
        }

        private Result<IEnumerable<object[]>, IActionResult> GetLookupData(HashtagCategory hashT, int limit, int pageNumber, string searchPhrase)
        {
            IDataLookupService lookupService
                    = ServiceManager.Services.GetService<IDataLookupService>();
            var internalRequest = new LookupListRequest
            {
                LookupId = hashT.LookupId,
                CurrentRow = null,
                ShowUniqueValues = false,
                SearchText = "%" + (string.IsNullOrEmpty(searchPhrase)?"":searchPhrase) + "%",
                PageSize = limit,
                PageNumber = pageNumber,
                ParameterMappings = null
            };
            var dataTable = lookupService.GetList(internalRequest);
            return AreColumnNamesValid(hashT.Lookup.ValueDisplayMember.Split(";"), dataTable)
                ? Result.Success<IEnumerable<object[]>, IActionResult>(
                    GetRowData(internalRequest, dataTable,GetListColumn(hashT)))
                : Result.Failure<IEnumerable<object[]>, IActionResult>(
                    BadRequest("Some of the supplied column names are not in the table."));
        }

        private string[] GetListColumn(HashtagCategory hashT)
        {
            string displayColumn = hashT.Lookup.ValueValueMember +";" +hashT.Lookup.ValueDisplayMember;
            return displayColumn.Split(";");
        }

        private IEnumerable<object[]> GetRowData(
            LookupListRequest input, DataTable dataTable, string[] columnNames)
        {
            var lookup = FindItem<DataServiceDataLookup>(input.LookupId).Value;
            if (lookup.IsFilteredServerside)
            {
                return dataTable.Rows
                    .Cast<DataRow>()
                    .Select(row => GetColumnValues(row, columnNames));
            }
            logger.LogError(string.Format("Lookup {0} has property IsFilteredServerSide set to false!",input.LookupId));
            return (IEnumerable<object[]>)BadRequest("Invalid lookup configuration. Data could not be retrieved. See log for more details.");
           
        }

        private static object[] GetColumnValues(
            DataRow row, IEnumerable<string> columnNames)
        {
            return columnNames.Select(colName => row[colName]).ToArray();
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

        private List<HashtagCategoryResult> GetCategories()
        {
            var hashtagProvider = GetHasTagProvider();
            var categories = hashtagProvider.ChildItems;
            List<HashtagCategoryResult> hashtagCategoryList = new List<HashtagCategoryResult>();
            foreach (HashtagCategory category in categories)
            {
                if (TestRole(category.Roles))
                {
                    var result = new HashtagCategoryResult
                    {
                        HashtagName = category.Name,
                        HashtagLabel = category.Label,
                        ObjectComboboxMetada = CreateComboBox(category)
                    };
                    hashtagCategoryList.Add(result);
                }
            }
            return hashtagCategoryList;
        }

        private bool TestRole(string roles)
        {
            return SecurityManager.GetAuthorizationProvider()
                .Authorize(SecurityManager.CurrentPrincipal, roles);
        }

        private string CreateComboBox(HashtagCategory category)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement controlElement = doc.CreateElement("control");
            doc.AppendChild(controlElement);
            ComboBoxBuilder.Build(controlElement, category.LookupId, false, null, null);
            return doc.InnerXml;
        }
    }
}
