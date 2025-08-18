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

using System.Collections.Generic;
using System.Collections;
using Origam.DA;

namespace Origam.Server;
public class UIRequest
{
    public bool SupportsPagedData { get; set; } = false;
    public IDictionary Parameters { get; set; } = new Hashtable();
    public List<string> CachedFormIds { get; set; } = new List<string>();
    public string FormSessionId { get; set; }
    public bool IsNewSession { get; set; } = true;
    public string ParentSessionId { get; set; }
    public string SourceActionId { get; set; }
    public bool IsStandalone { get; set; } = false;
    public bool IsDataOnly { get; set; } = false;
    public string TypeString => Type.ToString();
    public UIRequestType Type { get; set; }
    public string Caption { get; set; }
    public string Icon { get; set; }
    public string ObjectId { get; set; }
    public bool IsSingleRecordEdit { get; set; }
    public bool RequestCurrentRecordId { get; set; }
    public int DialogWidth { get; set; }
    public int DialogHeight { get; set; }
    public bool IsModalDialog { get; set; } = false;
    public bool RegisterSession { get; set; } = true;
    public bool DataRequested { get; set; } = true;
    public IDictionary NewRecordInitialValues { get; set; } 
    public QueryParameterCollection QueryParameters
    {
        get
        {
            QueryParameterCollection qparams = new QueryParameterCollection();
            foreach (DictionaryEntry entry in Parameters)
            {
                qparams.Add(new QueryParameter((string) entry.Key, entry.Value));
            }
            return qparams;
        }
    }
}
