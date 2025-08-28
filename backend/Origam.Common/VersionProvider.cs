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

namespace Origam.OrigamEngine;

public static class VersionProvider
{
    // only used by Database persistence (the old way), file persisted classes have their meta version persisted with them
    public static readonly string CurrentModelMetaVersion = "5.0.0";

    // only used by Database persistence (the old way), file persisted classes have their meta version persisted with them
    public static Version CurrentModelMeta { get; } = new Version(CurrentModelMetaVersion);
    public static readonly string CurrentPersistenceMetaVersion = "1.0.0";
    public static Version CurrentPersistenceMeta { get; } =
        new Version(CurrentPersistenceMetaVersion);

    public static bool IsCurrentMeta(string versionString)
    {
        if (string.IsNullOrEmpty(versionString))
        {
            return false;
        }

        Version version = new Version(versionString);
        if (version.Build == -1)
        {
            version = new Version(versionString + ".0");
        }
        return version.Equals(CurrentModelMeta);
    }
}
