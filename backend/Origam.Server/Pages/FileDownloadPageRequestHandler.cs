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
using System.Collections.Generic;
using System.Data;
using Origam.DA;
using Origam.Rule;
using Origam.Schema.GuiModel;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Pages;

class FileDownloadPageRequestHandler : AbstractPageRequestHandler
{
    private readonly IHttpTools httpTools;

    public FileDownloadPageRequestHandler(IHttpTools httpTools)
    {
        this.httpTools = httpTools;
    }

    public override void Execute(
        AbstractPage page,
        Dictionary<string, object> parameters,
        IRequestWrapper request,
        IResponseWrapper response
    )
    {
        FileDownloadPage fdPage = page as FileDownloadPage;
        QueryParameterCollection qparams = new QueryParameterCollection();
        Hashtable transformParams = new Hashtable();
        Hashtable preprocessorParams = GetPreprocessorParameters(request: request);
        // convert parameters to QueryParameterCollection for data service and hashtable for transformation service
        foreach (KeyValuePair<string, object> p in parameters)
        {
            qparams.Add(value: new QueryParameter(_parameterName: p.Key, value: p.Value));
            transformParams.Add(key: p.Key, value: p.Value);
        }
        // copy also the preprocessor parameters to the transformation parameters
        foreach (DictionaryEntry rp in preprocessorParams)
        {
            transformParams.Add(key: rp.Key, value: rp.Value);
        }
        RuleEngine ruleEngine = RuleEngine.Create(contextStores: null, transactionId: null);
        Validate(
            data: null,
            transformParams: transformParams,
            ruleEngine: ruleEngine,
            validation: fdPage.InputValidationRule
        );
        DataSet data = CoreServices.DataService.Instance.LoadData(
            dataStructureId: fdPage.DataStructureId,
            methodId: fdPage.DataStructureMethodId,
            defaultSetId: Guid.Empty,
            sortSetId: fdPage.DataStructureSortSetId,
            transactionId: null,
            parameters: qparams
        );
        DataTable table = data.Tables[index: 0];
        bool notFound = false;
        byte[] bytes = null;
        if (table.Rows.Count == 0)
        {
            notFound = true;
        }

        if (!notFound)
        {
            bytes = table.Rows[index: 0][columnName: fdPage.ContentField] as byte[];
            if (bytes == null)
            {
                throw new Exception(message: "Field is not a byte array.");
            }
            if (bytes.LongLength == 0)
            {
                notFound = true;
            }
        }
        if (!notFound)
        {
            string contentType = null;
            if (fdPage.MimeType != "?")
            {
                contentType = fdPage.MimeType;
            }
            httpTools.WriteFile(
                request: request,
                response: response,
                file: bytes,
                fileName: (string)table.Rows[index: 0][columnName: fdPage.FileNameField],
                isPreview: true,
                overrideContentType: contentType
            );
        }
        else
        {
            response.StatusCode = 404;
        }
    }
}
