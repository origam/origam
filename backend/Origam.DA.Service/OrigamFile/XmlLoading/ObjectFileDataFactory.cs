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


using System.Collections.Generic;

namespace Origam.DA.Service;
public class ObjectFileDataFactory
{
    private readonly OrigamFileFactory origamFileFactory;
    private readonly IList<string> parentFolders;
    public ObjectFileDataFactory(OrigamFileFactory origamFileFactory, IList<string> parentFolders)
    {
        this.origamFileFactory = origamFileFactory;
        this.parentFolders = parentFolders;
    }
    public PackageFileData NewPackageFileData(XmlFileData xmlData)
    {
        return new PackageFileData(parentFolders, xmlData, origamFileFactory); 
    }
    public GroupFileData NewGroupFileData(XmlFileData xmlData)
    {
        return new GroupFileData(parentFolders, xmlData, origamFileFactory);   
    }
    public ObjectFileData NewObjectFileData(XmlFileData xmlData)
    {
        return new ObjectFileData(new ParentFolders(parentFolders), xmlData, origamFileFactory); 
    }
    public ReferenceFileData NewReferenceFileData(XmlFileData xmlData)
    {
        return new ReferenceFileData(xmlData, origamFileFactory);
    }
}
