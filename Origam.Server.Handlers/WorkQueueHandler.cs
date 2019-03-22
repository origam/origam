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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using log4net;
using Origam.Server.Utils;
using Origam.Workbench.Services;
using Origam.Rule;
using Newtonsoft.Json;
using System.Data;

namespace Origam.Server.Handlers
{
	/// <summary>
	/// Run commands on workqueue entry
	/// </summary>
	/// <example>
	/// ?workQueueEntryId=aa4dc88b-3d80-4a55-bffc-fd9cc471b59c&commandId=aa4dc88b-3d80-4a55-bffc-fd9cc471b59c
	/// </example>
	public class WorkQueueHandler : OwinMiddleware
	{
		private static readonly ILog log
			= LogManager.GetLogger(typeof(WorkQueueHandler));

		public WorkQueueHandler(OwinMiddleware next) : base(next)
		{ }

		public override async Task Invoke(IOwinContext context)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("Processing: " + context.Request.Uri);
			}
			Dictionary<string, string> parameters
				= OwinContextHelper.GetRequestParameters(context.Request);


			/* parse parameters: 
			 * workQueueEntryId - guid of workqueue entry id
			 * commandId - id (guid) of command?
			 */
			Guid queueEntryId;
			Guid commandId;

			string resultContentType = "application/json; charset=utf-8";
			// set result content type according to content type of request
			if (context.Request.ContentType != "")
			{
				context.Response.ContentType = context.Request.ContentType;
			}

			try
			{
				if (!parameters.ContainsKey("workQueueEntryId"))
				{
					throw new RuleException(
						String.Format(Resources.ParameterIsMandatory,
						"workQueueEntryId"), RuleExceptionSeverity.High,
						"workQueueEntryId", "");
				}
				if (!Guid.TryParse(parameters["workQueueEntryId"], out queueEntryId))
				{
					throw new RuleException(
						String.Format(Resources.InvalidParameterType,
						"workQueueEntryId", "Guid"), RuleExceptionSeverity.High,
						"workQueueEntryId", "");
				}

				if (!parameters.ContainsKey("commandId"))
				{
					throw new RuleException(
						String.Format(Resources.ParameterIsMandatory,
						"commandId"), RuleExceptionSeverity.High,
						"commandId", "");
				}
				if (!Guid.TryParse(parameters["commandId"], out commandId))
				{
					throw new RuleException(
						String.Format(Resources.InvalidParameterType,
						"commandId", "Guid"), RuleExceptionSeverity.High,
						"commandId", "");
				}
				commandId = Guid.Parse(parameters["commandId"]);

				IWorkQueueService wqs = ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;			
				wqs.HandleAction(queueEntryId, commandId, true, null);
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled) log.Error(ex.Message, ex);
				if (log.IsDebugEnabled) log.DebugFormat("Result Content Type: {0}", resultContentType);

				string output;
				if (resultContentType.Substring(0,16) == "application/json")
				{
					if (ex is RuleException)
					{
						output = String.Format("{{\"Message\" : {0}, \"RuleResult\" : {1}}}",
							JsonConvert.SerializeObject(ex.Message),
							JsonConvert.SerializeObject(((RuleException)ex).RuleResult));
					}
					else if (ex is ArgumentOutOfRangeException)
					{
						ArgumentOutOfRangeException exArg = ex as ArgumentOutOfRangeException;
						output = String.Format("{{\"Message\" : {0}, \"ParamName\" : {1}, \"ActualValue\" : {2}}}"
							, JsonConvert.SerializeObject(exArg.Message)
							, JsonConvert.SerializeObject(exArg.ParamName)
							, JsonConvert.SerializeObject(exArg.ActualValue));
					
					}
					else
					{
						output = JsonConvert.SerializeObject(ex);
					}
				}
				else
				{
					output = ex.Message + ex.StackTrace;
				}
				context.Response.StatusCode = 400;
				context.Response.ContentType = resultContentType;
				context.Response.Write(output);
			}
			await Task.FromResult(0);
		}
	}
}
