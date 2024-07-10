#region license
/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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
using System.Xml.XPath;

namespace Origam.Rule.XsltFunctions;

public class LookupXsltFunctionContainer: AbstractOrigamDependentXsltFunctionContainer
{
    public string LookupValue(string lookupId, XPathNavigator parameters)
    {
        Hashtable parameterTable = ToHashtable(parameters);
        object result = LookupService.GetDisplayText(new Guid(lookupId),
            parameterTable, false, false, this.TransactionId);

        return XmlTools.FormatXmlString(result);
    }
    
    private Hashtable ToHashtable(XPathNavigator parametersNavigator)
    {
        Hashtable paramTable = new Hashtable();
        
        XPathNavigator paramNav = parametersNavigator.Clone();
        if (paramNav.MoveToFirstChild())
        {
            do
            {
                string name = paramNav.SelectSingleNode("name")?.Value;
                string value = paramNav.SelectSingleNode("value")?.Value;

                if (!string.IsNullOrEmpty(name))
                {
                    paramTable[name] = value;
                }
            } while (paramNav.MoveToNext());
        }
        return paramTable;
    }
}