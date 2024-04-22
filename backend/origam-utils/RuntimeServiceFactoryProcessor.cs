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
using Origam.OrigamEngine;
using Origam.Workbench.Services;

namespace Origam.Utils;

class RuntimeServiceFactoryProcessor : IRuntimeServiceFactory
{
    public IPersistenceService CreatePersistenceService()
    {
            return GetPersistenceBuilder().GetPersistenceService();
        }

    public IDocumentationService CreateDocumentationService()
    {
            return GetPersistenceBuilder().GetDocumentationService();
        }

    private static IPersistenceBuilder GetPersistenceBuilder()
    {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            string[] classpath = settings.ModelProvider.Split(',');
            return Reflector.InvokeObject(classpath[0], classpath[1]) as IPersistenceBuilder;
        }

    public void InitializeServices()
    {
            ServiceManager.Services.AddService(CreatePersistenceService());
            ServiceManager.Services.AddService(new SchemaService());
            ServiceManager.Services.AddService(new NullParameterService());
        }

    protected virtual IParameterService CreateParameterService()
    {
            return new ParameterService();
        }

    public void UnloadServices()
    {
            throw new System.NotImplementedException();
        }
}