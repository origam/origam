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

using System.IO;
using Origam.Extensions;

namespace Origam.DA.Service;

/// <summary>
///  This class wraps DirectoryInfo because we need it as a key in a
///  dictionary and DirectoryInfo doesn't override hashcode ands Equals.
/// </summary>
public class Folder
{
    private readonly DirectoryInfo dirInfo;

    public Folder(string path)
    {
            dirInfo = new DirectoryInfo(path); 
        }

    public Folder Parent => new Folder(dirInfo.Parent.FullName);

    public bool IsParentOf(Folder other) => 
        dirInfo.IsOnPathOf(other.dirInfo);

    private bool Equals(Folder other) => 
        string.Equals(dirInfo.FullName, other.dirInfo.FullName);

    public override bool Equals(object obj)
    {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Folder) obj);
        }

    public override int GetHashCode() => 
        (dirInfo.FullName != null ? dirInfo.FullName.GetHashCode() : 0);
}