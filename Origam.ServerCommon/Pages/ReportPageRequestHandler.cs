#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
﻿#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using core = Origam.Workbench.Services.CoreServices;
using System.Collections;
using Origam.Rule;

namespace Origam.ServerCommon.Pages
{
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
			RuleEngine ruleEngine = new RuleEngine(null, null);
			Validate(null, transformParams, ruleEngine, reportPage.InputValidationRule);

			// get report
			byte[] result = core.ReportService.GetReport(report.Id, null,
				reportPage.ExportFormatType.GetString(), hashParams, null);
			
			// set proper content type
			response.ContentType = "application/pdf";
			// write to response.OutputStream
			response.OutputStreamWrite(result, 0, result.Length);
		}
	}
}
