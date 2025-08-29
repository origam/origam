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

using System;
using System.Collections;
using System.Data;

namespace Origam.Services;

public class LookupListRequest
{
    public Guid LookupId = Guid.Empty;
    public string FieldName = null;
    public Hashtable ParameterMappings = new Hashtable();
    public DataRow CurrentRow = null;
    public bool ShowUniqueValues = false;
    public string SearchText = null;
    public string TransactionId = null;
    public int PageSize = -1;
    public int PageNumber = -1;
}
