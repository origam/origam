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

using System.Collections;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using Microsoft.AspNetCore.Http;
using Origam.Rule;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;

namespace Origam.Server.Pages;

class RedirectWorkflowPageActionHandler : AbstractWorkflowPageActionHandler
{
    public override void Execute(
        AbstractWorkflowPageAction action,
        object workflowResult,
        IRequestWrapper request,
        IResponseWrapper response
    )
    {
        RedirectWorkflowPageAction redirectAction = action as RedirectWorkflowPageAction;
        RuleEngine re = RuleEngine.Create(new Hashtable(), null);
        IXmlContainer doc = re.GetXmlDocumentFromData(workflowResult);
        XPathNavigator nav = doc.Xml.CreateNavigator();
        string url = XpathEvaluator.Instance.Evaluate(nav, redirectAction.XPath);
        Hashtable parameters = new Hashtable();
        foreach (
            var actionParameter in action.ChildItemsByType<WorkflowPageActionParameter>(
                WorkflowPageActionParameter.CategoryConst
            )
        )
        {
            string parameterResult = XpathEvaluator.Instance.Evaluate(nav, actionParameter.XPath);
            parameters.Add(actionParameter.Name, parameterResult);
        }
        string result = HttpTools.Instance.BuildUrl(
            url,
            parameters,
            false,
            "http",
            redirectAction.IsUrlEscaped
        );
        response.Redirect(result);
    }
}
