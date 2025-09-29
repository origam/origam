#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

namespace Origam.Server.Common;

public static class OrigamEvent
{
    public static readonly Guid DataStructureId = new("c35a0893-1b41-4a4a-a70d-795a088957ae");

    public static readonly (string FeatureCode, Guid EventId) SignIn = (
        "EVENT_SIGN_IN",
        new Guid("3d91cd2c-1265-4022-8834-150de0237a3c")
    );
    public static readonly (string FeatureCode, Guid EventId) SignOut = (
        "EVENT_SIGN_OUT",
        new Guid("b737ea8b-1cda-42d1-845a-c0c909386389")
    );
    public static readonly (string FeatureCode, Guid EventId) OpenScreen = (
        "EVENT_OPEN_SCREEN",
        new Guid("4c14c1da-797b-4f0f-b6df-f787ca7e9647")
    );
    public static readonly (string FeatureCode, Guid EventId) ExportToExcel = (
        "EVENT_EXPORT_TO_EXCEL",
        new Guid("62f161e9-b817-4631-93e3-360551257dc2")
    );
}
