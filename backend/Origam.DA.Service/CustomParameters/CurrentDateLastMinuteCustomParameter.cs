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

namespace Origam.DA.Service.CustomParameters;

/// <summary>
/// Returns current date's last minute (DD.MM.YYYY 23:59:59.999)
/// </summary>
public class CurrentDateLastMinuteCustomParameter : ICustomParameter
{
    public string Name
    {
        get { return "parCurrentDateLastMinute"; }
    }

    public object Evaluate(UserProfile profile)
    {
        DateTime now = DateTime.Now;
        return new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999);
    }
}
