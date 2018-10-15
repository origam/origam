#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

using Origam;
using System;
using System.IO;

namespace Origam.ProjectAutomation
{
    public class CopyServerFilesBuilder : AbstractBuilder
    {
        string _destinationFolder;

        public override string Name
        {
            get
            {
                return "Copy Server Files";
            }
        }

        public override void Execute(Project project)
        {
            string sourceFolder = Path.Combine(project.ServerTemplateFolder, "ServerApplication");
            _destinationFolder = project.BinFolder;
            DirectoryInfo dir = new DirectoryInfo(_destinationFolder);
            if(dir.Exists)
            {
                throw new Exception(
                    string.Format(
                    "Target folder {0} already exists. Cannot copy server files to an existing folder.", 
                    _destinationFolder));
            }
            FileTools.DirectoryCopy(sourceFolder, _destinationFolder, true);
        }

        public override void Rollback()
        {
            DirectoryInfo dir = new DirectoryInfo(_destinationFolder);
            dir.Delete(true);
        }
    }
}
