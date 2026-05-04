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
using System.IO;
using Origam.BI;
using Origam.Extensions;
using Origam.Rule;
using Origam.Schema.GuiModel;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Pages;

class ReportPageRequestHandler : AbstractPageRequestHandler
{
    public override void Execute(
        AbstractPage page,
        Dictionary<string, object> parameters,
        IRequestWrapper request,
        IResponseWrapper response
    )
    {
        ReportPage reportPage = page as ReportPage;
        AbstractReport report = reportPage.Report as AbstractReport;
        Hashtable hashParams = new Hashtable();
        Hashtable transformParams = new Hashtable();
        Hashtable preprocessorParams = GetPreprocessorParameters(request: request);
        // convert parameters to QueryParameterCollection for data service and hashtable for transformation service
        foreach (KeyValuePair<string, object> p in parameters)
        {
            hashParams.Add(key: p.Key, value: p.Value);
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
            validation: reportPage.InputValidationRule
        );
        // get report
        if (report is FileSystemReport reportstream)
        {
            StreamFileToOutput(
                reportstream: reportstream,
                mimeType: page.MimeType,
                response: response,
                hashParams: hashParams
            );
        }
        else
        {
            byte[] result = CoreServices.ReportService.GetReport(
                reportId: report.Id,
                data: null,
                format: reportPage.ExportFormatType.GetString(),
                parameters: hashParams,
                transactionId: null
            );
            // set proper content type
            response.ContentType = "application/pdf";
            // write to response.OutputStream
            response.OutputStreamWrite(buffer: result, offset: 0, count: result.Length);
        }
    }

    private void StreamFileToOutput(
        FileSystemReport reportstream,
        string mimeType,
        IResponseWrapper response,
        Hashtable hashParams
    )
    {
        response.ContentType = mimeType;
        ReportHelper.PopulateDefaultValues(report: reportstream, parameters: hashParams);
        string reportPath = ReportHelper.BuildFileSystemReportFilePath(
            filePath: reportstream.ReportPath,
            parameters: hashParams
        );
        try
        {
            if (
                ValidateFileName(
                    reportPath: reportstream.ReportPath,
                    fullpath: reportPath,
                    hashParams: hashParams
                )
            )
            {
                using (StreamReader sr = File.OpenText(path: reportPath))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        response.WriteToOutput(writeAction: textwriter =>
                            textwriter.WriteLine(value: s)
                        );
                    }
                }
            }
            else
            {
                response.WriteToOutput(writeAction: textwriter =>
                    textwriter.WriteLine(value: Resources.BlobFileNotAvailable)
                );
            }
        }
        catch (Exception)
        {
            response.WriteToOutput(writeAction: textwriter =>
                textwriter.WriteLine(value: Resources.BlobFileNotAvailable)
            );
        }
    }

    private bool ValidateFileName(string reportPath, string fullpath, Hashtable hashParams)
    {
        //determine working directory
        string workingDirectory = GetWorkingDirectory(
            reportPath: reportPath,
            hashParams: hashParams
        );
        string directoryOfFile = Path.GetDirectoryName(path: fullpath)
            .ReplaceInvalidFileCharacters(replaceWith: "");
        return workingDirectory != null && workingDirectory == directoryOfFile;
    }

    private string GetWorkingDirectory(string reportPath, Hashtable hashParams)
    {
        int firstbracket = reportPath.IndexOf(value: "{");
        if (firstbracket == -1)
        {
            if (Directory.Exists(path: Path.GetDirectoryName(path: reportPath)))
            {
                return Path.GetDirectoryName(path: reportPath)
                    .ReplaceInvalidFileCharacters(replaceWith: "");
            }
        }
        if (firstbracket == 0)
        {
            int secondBracket = reportPath.IndexOf(value: "}");
            string paramDefaultdirectory = reportPath.Substring(
                startIndex: 1,
                length: secondBracket - 1
            );
            if (hashParams.ContainsKey(key: paramDefaultdirectory))
            {
                string dir = (string)hashParams[key: paramDefaultdirectory];
                return dir.ReplaceInvalidFileCharacters(replaceWith: "");
            }
        }
        if (firstbracket > 0)
        {
            return reportPath
                .Substring(startIndex: 0, length: firstbracket)
                .ReplaceInvalidFileCharacters(replaceWith: "");
        }
        return null;
    }
}
