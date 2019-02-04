#region license
/*
Copyright 2005 - 2017 Advantage Solutions, s. r. o.

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
using Origam.Schema.WorkflowModel;
using System.Web;
using Origam.Rule;
using System.Xml.XPath;
using System.Xml;
using Origam;

namespace Origam.Server.Pages
{
    class RedirectWorkflowPageActionHandler : AbstractWorkflowPageActionHandler
    {
        public override void Execute(AbstractWorkflowPageAction action, object workflowResult, HttpRequest request, HttpResponse response)
        {
            RedirectWorkflowPageAction redirectAction = action as RedirectWorkflowPageAction;

            RuleEngine re = new RuleEngine(new Hashtable(), null);
            IXmlContainer doc = re.GetXmlDocumentFromData(workflowResult);
            XPathNavigator nav = doc.Xml.CreateNavigator();
            string url = re.EvaluateXPath(nav, redirectAction.XPath);

            Hashtable parameters = new Hashtable();

            foreach (WorkflowPageActionParameter actionParameter in action.ChildItemsByType(WorkflowPageActionParameter.ItemTypeConst))
            {
                string parameterResult = re.EvaluateXPath(nav, actionParameter.XPath);
                parameters.Add(actionParameter.Name, parameterResult);
            }

            string result = HttpTools.BuildUrl(url, parameters, false, "http", redirectAction.IsUrlEscaped);

            response.Redirect(result);
        }
    }
}
