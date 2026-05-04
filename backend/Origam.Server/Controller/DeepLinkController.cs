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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Server.Model.DeepLink;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Server.Controller;

[Authorize(Policy = "InternalApi")]
[ApiController]
[Route(template: "internalApi/[controller]")]
public class DeepLinkController : AbstractController
{
    private readonly ILogger<AbstractController> logger;

    public DeepLinkController(
        SessionObjects sessionObjects,
        IStringLocalizer<SharedResources> localizer,
        ILogger<AbstractController> log,
        IWebHostEnvironment environment
    )
        : base(log: log, sessionObjects: sessionObjects, environment: environment)
    {
        this.logger = log;
    }

    [HttpGet(template: "categories")]
    public IActionResult GetCategoriesRequest()
    {
        return Ok(value: GetCategories());
    }

    [HttpGet(template: "{categoryId}/objects")]
    public IActionResult GetObjectsRequest(
        string categoryId,
        [FromQuery] int limit,
        [FromQuery] int pageNumber,
        [FromQuery] string searchPhrase
    )
    {
        return GetObjets(
            categoryId: categoryId,
            limit: limit,
            pageNumber: pageNumber,
            searchPhrase: searchPhrase
        );
    }

    [HttpPost(template: "{categoryId}/labels")]
    public IActionResult GetLookupLabelRequest(
        string categoryId,
        [FromBody] DeepLinkLabelInput label
    )
    {
        return GetLookupLabel(categoryId: categoryId, labelIds: label.LabelIds);
    }

    private IActionResult GetLookupLabel(string categoryId, object[] labelIds)
    {
        DeepLinkCategory category = GetCategory(categoryId: categoryId);
        if (category == null)
        {
            return NotFound();
        }
        if (TestRole(roles: category.Roles))
        {
            var labelDictionary = GetLookupLabelsInternal(input: category, labelIds: labelIds);
            return Ok(value: labelDictionary);
        }
        return Forbid();
    }

    private DeepLinkCategory GetCategory(string categoryId)
    {
        var deepLinkProvider = GetDeepLinkProvider();
        DeepLinkCategory category =
            deepLinkProvider.GetChildByName(
                name: categoryId,
                itemType: DeepLinkCategory.CategoryConst
            ) as DeepLinkCategory;
        return category;
    }

    private Dictionary<object, string> GetLookupLabelsInternal(
        DeepLinkCategory input,
        object[] labelIds
    )
    {
        IDataLookupService lookupService = ServiceManager.Services.GetService<IDataLookupService>();
        var labelDictionary = labelIds.ToDictionary(
            keySelector: id => id,
            elementSelector: id =>
            {
                object lookupResult = lookupService.GetDisplayText(
                    lookupId: input.LookupId,
                    lookupValue: id,
                    useCache: false,
                    returnMessageIfNull: true,
                    transactionId: null
                );
                return lookupResult is decimal result
                    ? result.ToString(format: "0.#")
                    : lookupResult.ToString();
            }
        );
        return labelDictionary;
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetMenuId([FromBody] GetDeepLinkMenuInput input)
    {
        return Ok(
            value: GetMenuId(deepLinkCategory: input.Category, referenceId: input.ReferenceId)
        );
    }

    private string GetMenuId(string deepLinkCategory, object referenceId)
    {
        DeepLinkCategory linkCategory = GetCategory(categoryId: deepLinkCategory);
        if (linkCategory == null)
        {
            throw new Exception(message: $"deepLinkCategory: \"{deepLinkCategory}\" was not found");
        }
        return (
            ServiceManager
                .Services.GetService<IDataLookupService>()
                .GetMenuBinding(lookupId: linkCategory.LookupId, value: referenceId)
            ?? throw new Exception(
                message: $"deepLinkCategory: \"{deepLinkCategory}\" or ReferenceId: \"{referenceId}\" was not found"
            )
        ).MenuId;
    }

    private IActionResult GetObjets(
        string categoryId,
        int limit,
        int pageNumber,
        string searchPhrase
    )
    {
        DeepLinkCategory category = GetCategory(categoryId: categoryId);
        if (category != null)
        {
            var lookupData = GetLookupData(
                hashT: category,
                limit: limit,
                pageNumber: pageNumber,
                searchPhrase: searchPhrase
            );
            return lookupData.IsSuccess ? Ok(value: lookupData.Value) : lookupData.Error;
        }
        return NotFound();
    }

    private DeepLinkCategorySchemaItemProvider GetDeepLinkProvider()
    {
        var schemaservice = ServiceManager.Services.GetService<SchemaService>();
        return schemaservice.GetProvider<DeepLinkCategorySchemaItemProvider>();
    }

    private Result<IEnumerable<object[]>, IActionResult> GetLookupData(
        DeepLinkCategory hashT,
        int limit,
        int pageNumber,
        string searchPhrase
    )
    {
        IDataLookupService lookupService = ServiceManager.Services.GetService<IDataLookupService>();
        var internalRequest = new LookupListRequest
        {
            LookupId = hashT.LookupId,
            CurrentRow = null,
            ShowUniqueValues = false,
            SearchText = (string.IsNullOrEmpty(value: searchPhrase) ? "" : searchPhrase),
            PageSize = limit,
            PageNumber = pageNumber,
            ParameterMappings = null,
        };
        var dataTable = lookupService.GetList(request: internalRequest);
        return AreColumnNamesValid(
            columnNames: hashT.Lookup.ListDisplayMember.Split(separator: ";"),
            dataTable: dataTable
        )
            ? Result.Success<IEnumerable<object[]>, IActionResult>(
                value: GetRowData(
                    input: internalRequest,
                    dataTable: dataTable,
                    columnNames: GetListColumn(deepLinkCategory: hashT)
                )
            )
            : Result.Failure<IEnumerable<object[]>, IActionResult>(
                error: BadRequest(error: "Some of the supplied column names are not in the table.")
            );
    }

    private string[] GetListColumn(DeepLinkCategory deepLinkCategory)
    {
        string displayColumn =
            deepLinkCategory.Lookup.ListValueMember
            + ";"
            + deepLinkCategory.Lookup.ListDisplayMember;
        return displayColumn.Split(separator: ";");
    }

    private IEnumerable<object[]> GetRowData(
        LookupListRequest input,
        DataTable dataTable,
        string[] columnNames
    )
    {
        var lookup = FindItem<DataServiceDataLookup>(id: input.LookupId).Value;
        if (lookup.IsFilteredServerside)
        {
            return dataTable.Rows.Cast<DataRow>().Select(selector: row => row.ItemArray);
        }
        logger.LogError(
            message: string.Format(
                format: "Lookup {0} has property IsFilteredServerSide set to false!",
                arg0: input.LookupId
            )
        );
        return (IEnumerable<object[]>)
            BadRequest(
                error: "Invalid lookup configuration. Data could not be retrieved. See log for more details."
            );
    }

    private static bool AreColumnNamesValid(string[] columnNames, DataTable dataTable)
    {
        var actualColumnNames = dataTable
            .Columns.Cast<DataColumn>()
            .Select(selector: x => x.ColumnName)
            .ToArray();
        return columnNames.All(predicate: colName => actualColumnNames.Contains(value: colName));
    }

    private List<DeepLinkCategoryResult> GetCategories()
    {
        var deepLinkProvider = GetDeepLinkProvider();
        var categories = deepLinkProvider.ChildItems;
        List<DeepLinkCategoryResult> deepLinkCategoryList = new List<DeepLinkCategoryResult>();
        foreach (DeepLinkCategory category in categories)
        {
            if (TestRole(roles: category.Roles))
            {
                var result = new DeepLinkCategoryResult
                {
                    DeepLinkName = category.Name,
                    DeepLinkLabel = category.Label,
                    ObjectComboboxMetadata = CreateComboBox(category: category),
                };
                deepLinkCategoryList.Add(item: result);
            }
        }
        return deepLinkCategoryList;
    }

    private bool TestRole(string roles)
    {
        return SecurityManager
            .GetAuthorizationProvider()
            .Authorize(principal: SecurityManager.CurrentPrincipal, context: roles);
    }

    private string CreateComboBox(DeepLinkCategory category)
    {
        XmlDocument doc = new XmlDocument();
        XmlElement controlElement = doc.CreateElement(name: "control");
        doc.AppendChild(newChild: controlElement);
        ComboBoxBuilder.Build(
            propertyElement: controlElement,
            lookupId: category.LookupId,
            showUniqueValues: false,
            bindingMember: null,
            table: null
        );
        return doc.InnerXml;
    }
}
