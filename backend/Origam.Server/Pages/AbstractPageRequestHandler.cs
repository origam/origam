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
using System.Xml;
using Origam.Rule;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Service.Core;

namespace Origam.Server.Pages;

public abstract class AbstractPageRequestHandler : IPageRequestHandler
{
    public virtual void Execute(
        AbstractPage page,
        Dictionary<string, object> parameters,
        IRequestWrapper request,
        IResponseWrapper response
    )
    {
        throw new NotImplementedException();
    }

    internal static Hashtable GetPreprocessorParameters(IRequestWrapper request)
    {
        Hashtable preprocessorParams = new Hashtable();
        XmlDocument capabDoc = new XmlDocument();
        XmlElement capabilities = capabDoc.CreateElement(name: "BrowserCapabilities");
        capabDoc.AppendChild(newChild: capabilities);
        foreach (DictionaryEntry capEntry in request.BrowserCapabilities)
        {
            XmlElement capability = capabDoc.CreateElement(name: "Capability");
            capability.SetAttribute(name: "Name", value: capEntry.Key.ToString());
            capability.SetAttribute(
                name: "Value",
                value: Origam.XmlTools.ConvertToString(val: capEntry.Value)
            );
            capabilities.AppendChild(newChild: capability);
        }
        preprocessorParams.Add(key: "UserAgent", value: request.UserAgent);
        if (request.UserLanguages != null)
        {
            preprocessorParams.Add(
                key: "UserLanguages",
                value: string.Join(separator: ";", values: request.UserLanguages)
            );
        }
        preprocessorParams.Add(key: "BrowserCapabilities", value: capabilities);
        preprocessorParams.Add(key: "HttpMethod", value: request.HttpMethod);
        return preprocessorParams;
    }

    protected static void Validate(
        IXmlContainer data,
        Hashtable transformParams,
        RuleEngine ruleEngine,
        IEndRule validation
    )
    {
        if (validation != null)
        {
            RuleExceptionDataCollection result = ruleEngine.EvaluateEndRule(
                rule: validation,
                data: data,
                parameters: transformParams
            );
            // if there are some exceptions, we actually throw them
            if (result != null && result.Count != 0)
            {
                throw new RuleException(result: result);
            }
        }
    }
}
