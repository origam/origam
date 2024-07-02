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

using System;
using System.Collections.Generic;
using Origam.Schema.GuiModel;
using System.Web;
using Origam.DA;
using Origam.Schema.WorkflowModel;
using core = Origam.Workbench.Services.CoreServices;
using System.Collections;
using Origam.Rule;
using System.Xml;
using Origam.Schema;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Server.Pages;
class WorkflowPageRequestHandler : AbstractPageRequestHandler
{
    public override void Execute(AbstractPage page, Dictionary<string, object> parameters, IRequestWrapper request, IResponseWrapper response)
    {
        WorkflowPage workflowPage = page as WorkflowPage;
        QueryParameterCollection qparams = new QueryParameterCollection();
		Hashtable transformParams = new Hashtable();
		Hashtable preprocessorParams = GetPreprocessorParameters(request);
		// convert parameters to QueryParameterCollection for data service and hashtable for transformation service
		foreach (KeyValuePair<string, object> p in parameters)
		{
			qparams.Add(new QueryParameter(p.Key, p.Value));
			transformParams.Add(p.Key, p.Value);
		}
		// copy also the preprocessor parameters to the transformation parameters
		foreach (DictionaryEntry rp in preprocessorParams)
		{
			transformParams.Add(rp.Key, rp.Value);
		}
		RuleEngine ruleEngine = RuleEngine.Create(null, null);
		Validate(null, transformParams, ruleEngine, workflowPage.InputValidationRule);
		if (workflowPage.DisableConstraintForInputValidation)
		{
			// reenable constraints for context parameter
			foreach (KeyValuePair<string, object> p in parameters)
			{
				if (p.Value as IDataDocument != null)
				{
					(p.Value as IDataDocument).DataSet.EnforceConstraints = true;
				}
			}
		}
		object workflowResult = core.WorkflowService.ExecuteWorkflow(workflowPage.WorkflowId, qparams, null);
        bool handled = false;
        List<ISchemaItem> actions = workflowPage.ChildItemsByType(AbstractWorkflowPageAction.CategoryConst);
        actions.Sort();
        RuleEngine re = RuleEngine.Create(new Hashtable(), null);
        foreach (AbstractWorkflowPageAction action in actions)
        {
            bool conditionResult = true;
            if(action.ConditionRule != null)
            {
                conditionResult = (bool)re.EvaluateRule(action.ConditionRule, workflowResult, null);
            }
            if (conditionResult && IsInRole(action.Roles) && IsFeatureOn(action.Features))
            {
                IWorkflowPageActionHandler handler;
                if (action is RedirectWorkflowPageAction)
                {
                    handler = new RedirectWorkflowPageActionHandler();
                    handled = true;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("action", action, Resources.ErrorUnknownWorkflowPageAction);
                }
                handler.Execute(action, workflowResult, request, response);
                
                // execute the first valid action
                break;
            }
        }
        if (!handled)
        {
            if (!string.IsNullOrEmpty(request.UrlReferrerAbsoluteUri))
            {
                response.Redirect(request.UrlReferrerAbsoluteUri);
            }
        }
    }
    private bool IsFeatureOn(string featureCode)
    {
        return ServiceManager.Services
	        .GetService<IParameterService>()
	        .IsFeatureOn(featureCode);
    }
    private bool IsInRole(string roleName)
    {
        return SecurityManager
	        .GetAuthorizationProvider()
	        .Authorize(SecurityManager.CurrentPrincipal, roleName);
    }
}
