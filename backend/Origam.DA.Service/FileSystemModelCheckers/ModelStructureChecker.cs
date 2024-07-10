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
using System.IO;
using System.Linq;
using Origam.DA.Service.FileSystemModeCheckers;

namespace Origam.DA.Service;
public class ModelStructureChecker : IFileSystemModelChecker
{
    private readonly DirectoryInfo topDirectory;
    public ModelStructureChecker(DirectoryInfo topDirectory)
    {
        this.topDirectory = topDirectory;
    }
    public IEnumerable<ModelErrorSection> GetErrors()
    {
        List<ErrorMessage> errors = topDirectory
            .GetFiles(".origamPackage", SearchOption.AllDirectories)
            .Where(packageFile => !IsOneLevelBelowTopDirectory(packageFile))
            .Select(packageFile => 
                new ErrorMessage(
                    text: packageFile.FullName, 
                    link:packageFile.FullName)
            )
            .ToList();
        
        yield return new ModelErrorSection(
            "The following package files are not one level below the model directory. " +
            "This indicates the model directory is wrong. Please adjust it to avoid damage to the model structure!",
            errors);
    }
    private bool IsOneLevelBelowTopDirectory(FileInfo file)
    {
        return file.Directory?.Parent?.FullName == topDirectory.FullName;
    }
}
