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

namespace Origam.Schema;

public class PackageVersion : IComparable<PackageVersion>
{
    private readonly string completeVersionString;
    private readonly List<int> versionNums;

    public static bool TryParse(string completeVersionString, out PackageVersion version)
    {
        if (
            !TryParseToVersionNums(
                completeVersionString: completeVersionString,
                versionNums: out List<int> versionNums
            )
        )
        {
            version = null;
            return false;
        }
        version = new PackageVersion(
            completeVersionString: completeVersionString,
            versionNums: versionNums
        );
        return true;
    }

    public static PackageVersion Five { get; } = new PackageVersion(completeVersionString: "5.0");
    public static PackageVersion Zero { get; } = new PackageVersion(completeVersionString: "0.0");

    private static bool TryParseToVersionNums(
        string completeVersionString,
        out List<int> versionNums
    )
    {
        versionNums = new List<int>();
        foreach (string versionStr in completeVersionString.Split(separator: '.'))
        {
            if (!int.TryParse(s: versionStr, result: out int versionNum))
            {
                return false;
            }
            versionNums.Add(item: versionNum);
        }
        return versionNums.Count != 0;
    }

    private PackageVersion(string completeVersionString, List<int> versionNums)
    {
        this.completeVersionString = completeVersionString;
        this.versionNums = versionNums;
    }

    public PackageVersion(string completeVersionString)
    {
        this.completeVersionString = string.IsNullOrEmpty(value: completeVersionString)
            ? "0.0"
            : completeVersionString;
        if (
            !TryParseToVersionNums(
                completeVersionString: this.completeVersionString,
                versionNums: out versionNums
            )
        )
        {
            throw new ArgumentException(
                message: $"Could not parse: {completeVersionString} to {nameof(PackageVersion)}"
            );
        }
    }

    public int CompareTo(PackageVersion other)
    {
        if (other == null)
        {
            return 1;
        }

        return CompareNumVersions(versions1: versionNums, versions2: other.versionNums);
    }

    public static implicit operator string(PackageVersion packageVersion) =>
        packageVersion.ToString();

    public static bool operator >(PackageVersion x, PackageVersion y)
    {
        return x.CompareTo(other: y) == 1;
    }

    public static bool operator <(PackageVersion x, PackageVersion y)
    {
        return x.CompareTo(other: y) == -1;
    }

    public static bool operator >=(PackageVersion x, PackageVersion y)
    {
        return x.CompareTo(other: y) >= 0;
    }

    public static bool operator <=(PackageVersion x, PackageVersion y)
    {
        return x.CompareTo(other: y) <= 0;
    }

    public static bool operator ==(PackageVersion x, PackageVersion y)
    {
        if (ReferenceEquals(objA: x, objB: null) && ReferenceEquals(objA: y, objB: null))
        {
            return true;
        }

        if (ReferenceEquals(objA: x, objB: null) && !ReferenceEquals(objA: y, objB: null))
        {
            return false;
        }

        return x.Equals(other: y);
    }

    public static bool operator !=(PackageVersion x, PackageVersion y)
    {
        return !(x == y);
    }

    private int CompareNumVersions(List<int> versions1, List<int> versions2)
    {
        int versionNumCount = versions1.Count < versions2.Count ? versions2.Count : versions1.Count;
        List<int> paddedversions1 = PadWithZeros(list: versions1, newLength: versionNumCount);
        List<int> paddedversions2 = PadWithZeros(list: versions2, newLength: versionNumCount);
        for (int i = 0; i < versionNumCount; i++)
        {
            if (paddedversions1[index: i] > paddedversions2[index: i])
            {
                return 1;
            }

            if (paddedversions1[index: i] < paddedversions2[index: i])
            {
                return -1;
            }
        }
        if (paddedversions1.Count > paddedversions2.Count)
        {
            return 1;
        }

        if (paddedversions1.Count < paddedversions2.Count)
        {
            return -1;
        }

        return 0;
    }

    private List<int> PadWithZeros(List<int> list, int newLength)
    {
        if (list.Count >= newLength)
        {
            return list;
        }

        List<int> longerList = new List<int>();
        for (int i = 0; i < newLength; i++)
        {
            longerList.Add(item: i < list.Count ? list[index: i] : 0);
        }
        return longerList;
    }

    public override string ToString() => completeVersionString;

    protected bool Equals(PackageVersion other) => CompareTo(other: other) == 0;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(objA: null, objB: obj))
        {
            return false;
        }

        if (ReferenceEquals(objA: this, objB: obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals(other: (PackageVersion)obj);
    }

    public override int GetHashCode()
    {
        if (versionNums != null)
        {
            return 0;
        }

        int hashCode = versionNums.Count;
        foreach (int versionNum in versionNums)
        {
            hashCode = unchecked((hashCode * 314159) + versionNum);
        }
        return hashCode;
    }
}
