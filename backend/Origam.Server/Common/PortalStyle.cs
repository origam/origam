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
using System.Collections.Generic;
using System.Data;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class PortalStyle
{
    public IDictionary<string, int> Colors
    {
        get
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            DataSet ds = CoreServices.DataService.Instance.LoadData(
                new Guid("5a98c98f-d930-4a94-a13e-82685bb6dc29"),
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                null
            );
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                result.Add((string)row["Name"], (int)row["Color"]);
            }
            return result;
        }
    }
}
