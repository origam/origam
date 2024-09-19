#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Generic;
using Origam.Schema.GuiModel;
using CoreServices = Origam.Workbench.Services.CoreServices;
using System.Collections;
using Origam.Rule;
using Origam.BI;
using System;
using System.IO;
using Origam.Extensions;

namespace Origam.Server.Pages;
class ReportPageRequestHandler : AbstractPageRequestHandler
{
	public override void Execute(AbstractPage page, Dictionary<string, object> parameters, IRequestWrapper request, IResponseWrapper response)
	{
		ReportPage reportPage = page as ReportPage;
		AbstractReport report = reportPage.Report as AbstractReport;
		Hashtable hashParams = new Hashtable();
		Hashtable transformParams = new Hashtable();
		Hashtable preprocessorParams = GetPreprocessorParameters(request);
		// convert parameters to QueryParameterCollection for data service and hashtable for transformation service
		foreach (KeyValuePair<string, object> p in parameters)
		{
			hashParams.Add(p.Key, p.Value);
			transformParams.Add(p.Key, p.Value);
		}
		// copy also the preprocessor parameters to the transformation parameters
		foreach (DictionaryEntry rp in preprocessorParams)
		{
			transformParams.Add(rp.Key, rp.Value);
		}
		RuleEngine ruleEngine = RuleEngine.Create(null, null);
		Validate(null, transformParams, ruleEngine, reportPage.InputValidationRule);
		// get report
		if (report is FileSystemReport reportstream)
		{
			StreamFileToOutput(reportstream, page.MimeType, response, hashParams);
		}
		else
		{
			byte[] result = CoreServices.ReportService.GetReport(report.Id, null,
			reportPage.ExportFormatType.GetString(), hashParams, null);
			// set proper content type
			response.ContentType = "application/pdf";
			// write to response.OutputStream
			response.OutputStreamWrite(result, 0, result.Length);
		}
	}
	private void StreamFileToOutput(FileSystemReport reportstream, string mimeType, IResponseWrapper response, Hashtable hashParams)
	{
		response.ContentType = mimeType;
		ReportHelper.PopulateDefaultValues(
			reportstream, hashParams);
		string reportPath = ReportHelper.BuildFileSystemReportFilePath(reportstream.ReportPath, hashParams);
		try
		{
			if (ValidateFileName(reportstream.ReportPath, reportPath, hashParams))
			{
				using (StreamReader sr = File.OpenText(reportPath))
				{
					string s = "";
					while ((s = sr.ReadLine()) != null)
					{
						response.WriteToOutput(textwriter => textwriter.WriteLine(s));
					}
				}
			}
			else
			{
				response.WriteToOutput(textwriter => textwriter.WriteLine(Resources.BlobFileNotAvailable));
			}
		}
		catch (Exception)
		{
			response.WriteToOutput(textwriter => textwriter.WriteLine(Resources.BlobFileNotAvailable));
		}
	}
	private bool ValidateFileName(string reportPath, string fullpath, Hashtable hashParams)
	{
		//determine working directory 
		string workingDirectory = GetWorkingDirectory(reportPath, hashParams);
		string directoryOfFile = Path.GetDirectoryName(fullpath).ReplaceInvalidFileCharacters("");
		return workingDirectory != null && workingDirectory == directoryOfFile;
	}
	private string GetWorkingDirectory(string reportPath, Hashtable hashParams)
	{
		int firstbracket = reportPath.IndexOf("{");
		if (firstbracket == -1)
		{
			if (Directory.Exists(Path.GetDirectoryName(reportPath)))
			{
				return Path.GetDirectoryName(reportPath).ReplaceInvalidFileCharacters("");
			}
		}
		if (firstbracket == 0)
		{
			int secondBracket = reportPath.IndexOf("}");
			string paramDefaultdirectory = reportPath.Substring(1, secondBracket - 1);
			if (hashParams.ContainsKey(paramDefaultdirectory))
			{
				string dir = (string)hashParams[paramDefaultdirectory];
				return dir.ReplaceInvalidFileCharacters("");
			}
		}
		if (firstbracket > 0)
		{
			return reportPath.Substring(0, firstbracket).ReplaceInvalidFileCharacters("");
		}
		return null;
	}
}
