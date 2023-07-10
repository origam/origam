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
using System.Collections;
using System.Data;
using Origam.DA.ObjectPersistence;
using Origam.Schema;

namespace Origam.Workbench.Services
{
    public class NullPersistenceService : IPersistenceService
    {
        public void InitializeService()
        {
        }

        public void UnloadService()
        {
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public IPersistenceProvider SchemaProvider => new NullPersistenceProvider();
        public IPersistenceProvider SchemaListProvider => new NullPersistenceProvider();
        
        public Package LoadSchema(Guid schemaExtension, bool loadDocumentation, bool loadDeploymentScripts,
            string transactionId)
        {
            throw new NotImplementedException();
        }

        public Package LoadSchema(Guid schemaExtensionId)
        {
            throw new NotImplementedException();
        }

        public void InitializeRepository()
        {
        }

        public void Dispose()
        {
        }
    }
}