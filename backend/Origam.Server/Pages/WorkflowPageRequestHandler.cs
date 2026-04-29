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
using System.Collections;
using System.Collections.Generic;
using Origam.DA;
using Origam.Rule;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Common;
using Origam.Service.Core;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Pages;

class WorkflowPageRequestHandler : AbstractPageRequestHandler
{
    public override void Execute(
        AbstractPage page,
        Dictionary<string, object> parameters,
        IRequestWrapper request,
        IResponseWrapper response
    )
    {
        WorkflowPage workflowPage = page as WorkflowPage;
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
            validation: workflowPage.InputValidationRule
        );
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
        object workflowResult = CoreServices.WorkflowService.ExecuteWorkflow(
            workflowId: workflowPage.WorkflowId,
            parameters: qparams,
            transactionId: null
        );
        bool handled = false;
        var actions = workflowPage.ChildItemsByType<AbstractWorkflowPageAction>(
            itemType: AbstractWorkflowPageAction.CategoryConst
        );
        actions.Sort();
        RuleEngine re = RuleEngine.Create(contextStores: new Hashtable(), transactionId: null);
        foreach (AbstractWorkflowPageAction action in actions)
        {
            bool conditionResult = true;
            if (action.ConditionRule != null)
            {
                conditionResult = (bool)
                    re.EvaluateRule(
                        rule: action.ConditionRule,
                        data: workflowResult,
                        contextPosition: null
                    );
            }
            if (
                conditionResult
                && SecurityTools.IsInRole(roleName: action.Roles)
                && FeatureTools.IsFeatureOn(featureCode: action.Features)
            )
            {
                IWorkflowPageActionHandler handler;
                if (action is RedirectWorkflowPageAction)
                {
                    handler = new RedirectWorkflowPageActionHandler();
                    handled = true;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "action",
                        actualValue: action,
                        message: Resources.ErrorUnknownWorkflowPageAction
                    );
                }
                handler.Execute(
                    action: action,
                    workflowResult: workflowResult,
                    request: request,
                    response: response
                );

                // execute the first valid action
                break;
            }
        }
        if (!handled)
        {
            if (!string.IsNullOrEmpty(value: request.UrlReferrerAbsoluteUri))
            {
                response.Redirect(requestUrlReferrerAbsolutePath: request.UrlReferrerAbsoluteUri);
            }
        }
    }
}
