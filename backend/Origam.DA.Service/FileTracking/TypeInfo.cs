using System;
using ProtoBuf;
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

namespace Origam.DA.Service;
[ProtoContract]
public class TypeInfo
{
    [ProtoMember(1)]
    public string FullTypeName { get;  }
    public Version Version {
        get
        {
            if (version == null)
            {
                version = Version.Parse(versionStr);
            }
            return version;
        }
    }
    private Version version;
    [ProtoMember(2)]
    private readonly string versionStr;
    public TypeInfo()
    {
    }
    public TypeInfo( string fullTypeName, Version version)
    {
        versionStr =  version.ToString();
        FullTypeName = fullTypeName;
    }
    protected bool Equals(TypeInfo other)
    {
        return versionStr == other.versionStr && FullTypeName == other.FullTypeName;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TypeInfo) obj);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            return ((versionStr != null ? versionStr.GetHashCode() : 0) * 397) ^ (FullTypeName != null ? FullTypeName.GetHashCode() : 0);
        }
    }
}
