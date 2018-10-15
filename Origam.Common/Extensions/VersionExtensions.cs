using System;

namespace Origam.Extensions
{
    public static class VersionExtensions
    {
        public static bool DiffersOnlyInBuildFrom(this Version thisVersion, Version otherVersion)
        {
            return thisVersion.Minor == otherVersion.Minor &&
                   thisVersion.Major == otherVersion.Major;
        }
    }
}