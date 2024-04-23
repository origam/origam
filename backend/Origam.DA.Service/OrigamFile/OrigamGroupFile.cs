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
using System.IO;
using System.Linq;
using System.Xml;
using Origam.DA.ObjectPersistence;

namespace Origam.DA.Service;

class OrigamGroupFile : OrigamFile
{
    public OrigamGroupFile(OrigamPath path, IDictionary<string, Guid> parentFolderIds,
        OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
        FileEventQueue fileEventQueue, bool isAFullyWrittenFile = false) 
        : base(path, parentFolderIds, origamFileManager,origamPathFactory, fileEventQueue, isAFullyWrittenFile)
    {
        }

    public OrigamGroupFile(OrigamPath path, IDictionary<string, Guid> parentFolderIds,
        OrigamFileManager origamFileManager, OrigamPathFactory origamPathFactory,
        FileEventQueue fileEventQueue, string fileHash) 
        : base(path, parentFolderIds, origamFileManager,origamPathFactory, fileEventQueue, fileHash)
    {
        }

    protected override DirectoryInfo ReferenceFileDirectory =>
        Path.Directory.Parent;

    public override void WriteInstance(IFilePersistent instance)
    {
            XmlNode contentNode = DeferredSaveDocument.ChildNodes[1];
            bool anotherGroupPresent = contentNode.ChildNodes
                .Cast<XmlNode>()
                .Where(node => node.Name == "group")
                .Any(node => Guid.Parse(node.Attributes["x:id"].Value) != instance.Id);

            if (anotherGroupPresent)
            {
                throw new InvalidOperationException("Single .origamGroup file can contain only one group");
            }
            base.WriteInstance(instance);
        }
}