#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
﻿using Origam.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam.ProjectAutomation.Builders
{
    class GitBuilder : AbstractBuilder
    {
        public override string Name => "Initialize Git";
        public override void Execute(Project project)
        {
            if (project.GitRepository)
            {
                GitManager.CreateRepository(project.SourcesFolder);
                GitManager gitmanager = new GitManager(project.SourcesFolder);
                gitmanager.Init(project.Gitusername,project.Gitemail);
            }
        }

        public override void Rollback()
        {
            OrigamEngine.OrigamEngine.UnloadConnectedServices();
        }
    }
}
