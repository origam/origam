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
using Origam.Workbench.Services;

namespace Origam.ProjectAutomation;
public class FileModelInitBuilder:AbstractBuilder
{
    public override string Name => "Initialize model";
    private SchemaService schema = null;
    public override void Execute(Project project)
    {
        OrigamEngine.OrigamEngine.InitializeRuntimeServices();
        LoadBaseSchema(project);
    }
    private void LoadBaseSchema(Project project)
    {
        schema =
            ServiceManager.Services.GetService<SchemaService>();
        try
        {
            schema.LoadSchema(new Guid(project.BasePackageId), isInteractive: true);
        } catch
        {
            try
            {
                // In case something went wrong AFTER the model was loaded
                // (e.g. Architect failed handling some events) we unload the model.
                // Since we do not know if it failed really AFTER, we just catch
                // possible exceptions.
                schema.UnloadSchema();
                Rollback();
            } catch
            {
            }
            throw;
        }
    }
    public override void Rollback()
    {
        schema.UnloadSchema();
        OrigamEngine.OrigamEngine.UnloadConnectedServices();
    }
}
