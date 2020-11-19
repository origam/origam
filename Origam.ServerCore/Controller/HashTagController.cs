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
            [FromQuery] int pageNumber,
            [FromQuery] string searchPhrase)
        {
            return RunWithErrorHandler(() =>
                Ok(GetObjets(categoryId,limit,pageNumber,searchPhrase)));
        }

        [HttpPost("{categoryId}/labels")]
        public IActionResult GetLookupLabelRequest(string categoryId,[FromBody] HashTagLabel label)
        {
            return RunWithErrorHandler(() =>
                Ok(GetLookupLabel(categoryId,label.LabelIds)));
        }

        private object GetLookupLabel(string categoryId, object[] labelIds)
        {
            HashTag hashTag = GetHashtag(categoryId);
            if (TestRole(hashTag.Roles))
            {
                var labelDictionary = GetLookupLabelsInternal(hashTag, labelIds);
                return Ok(labelDictionary);
            }
            throw new Exception("No rights");
        }

        private HashTag GetHashtag(string categoryId)
        {
            var hashtagProvider = GetHasTagProvider();
            HashTag hashTag = hashtagProvider.GetChildByName(categoryId, HashTag.CategoryConst) as HashTag;
            return hashTag;
        }

        private Dictionary<object, string> GetLookupLabelsInternal(
           HashTag input, object[] labelIds)
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

        private object GetMenuId(string hashtagCategory, Guid ReferenceId)
        {
            var hashTag = GetHashtag(hashtagCategory);
            ArrayList datalookupMenuBinding = hashTag.Lookup.ChildItemsByType(DataLookupMenuBinding.CategoryConst);
            if(datalookupMenuBinding.Count!=1)
            {
                return new Exception("Problem with MenuBinding!");
            }
            if (datalookupMenuBinding.Count == 1 && TestRole(datalookupMenuBinding.Cast<DataLookupMenuBinding>().First().Roles))
            {
                var menuBinding = ServiceManager.Services
                .GetService<IDataLookupService>()
                .GetMenuBinding(GetHashtag(hashtagCategory).LookupId, ReferenceId);
                return menuBinding.MenuId;
            }
            return new Exception("No rights");
        }

        private object GetObjets(string categoryId, int limit, int pageNumber, string searchPhrase)
        {
            HashTag hashTag = GetHashtag(categoryId);
            if (hashTag!=null)
            {
                 return GetLookupData(hashTag,limit,pageNumber,searchPhrase);
            }
            return new Exception("No Data");
        }

        private HashTagSchemaItemProvider GetHasTagProvider()
        {
            var schemaservice = ServiceManager.Services.GetService<SchemaService>();
            return schemaservice.GetProvider<HashTagSchemaItemProvider>();
        }

        private object GetLookupData(HashTag hashT, int limit, int pageNumber, string searchPhrase)
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

        private string[] GetListColumn(HashTag hashT)
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

            throw new Exception("Lookup has property IsFilteredServerSide set to false !");
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

        private List<HashTagCategory> GetCategories()
        {
            var hashtagProvider = GetHasTagProvider();
            var hashTagcollection = hashtagProvider.ChildItems;

            List<HashTagCategory> hashtagCategoryList = new List<HashTagCategory>();
            foreach (HashTag hashTag in hashTagcollection)
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
