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

namespace Origam.DA.Common
{
    public class ClassMetaVersionAttribute : Attribute
    {
        public static readonly Version FirstVersion = new Version("1.0.0");
        // The first version used to be 6.0.0, to differentiate the new class
        // versions from old namespace versions in the xml files which ranged
        // from 1.0.0 to 5.0.0. It was later decided to change the first (minimum)
        // version to 1.0.0 
        public static readonly Version FormerFirstVersion = new Version("6.0.0");
        public Version Value { get; }

        public ClassMetaVersionAttribute(string versionStr)
        {
            var version = new Version(versionStr);
            if (version < FirstVersion)
            {
                throw new ArgumentException($"Cannot set class version to {version}. The minimum is {FirstVersion}");
            }

            Value = version;
        }
    }
}