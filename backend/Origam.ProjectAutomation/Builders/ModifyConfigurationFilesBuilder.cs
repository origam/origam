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

using Antlr4.StringTemplate;
using System.IO;

namespace Origam.ProjectAutomation;

public class ModifyConfigurationFilesBuilder : AbstractBuilder
{
    public override string Name
    {
        get
        {
                return "Modify Configuration Files";
            }
    }

    public override void Execute(Project project)
    {
            DirectoryInfo dir = new DirectoryInfo(project.BinFolder);
            foreach (string filter in new string[] {"*.config", "Startup.cs"})
            {
                FileInfo[] files = dir.GetFiles(filter, SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    Template template = new Template(System.IO.File.ReadAllText(file.FullName), '$', '$');
                    template.Add("project", project);
                    string result = template.Render();
                    File.WriteAllText(file.FullName, result);
                }
            }
        }

    public override void Rollback()
    {
        }
}