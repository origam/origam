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
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using Origam.OrigamEngine;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.DA.Service_net2Tests;
internal class PersitHelper
{
    private readonly IPersistenceService persistenceService;
    public IList<string> DefaultFolders { get; }
    
    public PersitHelper(string testFolderPath)
    {
        DefaultFolders = new List<string>
        {
            CategoryFactory.Create(typeof(Package)),
            CategoryFactory.Create(typeof(SchemaItemGroup))
        };
        persistenceService = new FilePersistenceService(
            metaModelUpgradeService: new NullMetaModelUpgradeService(), 
            defaultFolders: DefaultFolders, 
            pathToRuntimeModelConfig: "runtimeConfig.json" ,
            basePath: testFolderPath);
    }
    public IPersistenceProvider GetPersistenceProvider()
    {
        return persistenceService.SchemaProvider;
    }
    public void PersistAll()
    {
        PersistFolders<Package>();
        PersistFolders<SchemaItemGroup>();
        using (new DebugTimer())
        {
            TypeTools.AllProviderTypes
                .ForEach(PersistAllProviderItems);
        }
        var filePresProvider =
            (FilePersistenceProvider)persistenceService.SchemaProvider;
        filePresProvider.PersistIndex();
    }
    public IFilePersistent RetrieveSingle(Type type, Key primaryKey)
    {
        return (IFilePersistent)persistenceService.SchemaProvider
            .RetrieveInstance(type, primaryKey);
    }
    public List<AbstractSchemaItem> RetrieveAll()
    {
        List<AbstractSchemaItem> abstractSchemaItems = TypeTools.AllProviderTypes
            .Select(TypeTools.GetAllItems)
            .SelectMany(itemCollection => itemCollection.ToGeneric())
            .ToList();
        return abstractSchemaItems;
    }

    private void PersistFolders<T>()
    {
        IPersistenceService dbSvc = ServiceManager.Services.GetService(
            typeof(IPersistenceService)) as IPersistenceService;
        var listOfItems = dbSvc.SchemaProvider.RetrieveList<T>(null);
   
        foreach (IPersistent item in listOfItems)
        {
            persistenceService.SchemaProvider.Persist(item);
        }
    }
    private void PersistAllProviderItems(Type providerType)
    {
        var allItems = TypeTools.GetAllItems(providerType); 
        foreach (AbstractSchemaItem item in allItems)
        {
            persistenceService.SchemaProvider.BeginTransaction();
            Persist(item);
            persistenceService.SchemaProvider.EndTransaction();
        }
        Console.WriteLine("ProviderType:" + providerType +", items: "+allItems.Count);
    }
    public void Persist(List<IFilePersistent> items)
    {
        persistenceService.SchemaProvider.BeginTransaction();
        foreach (var item in items)
        {
            persistenceService.SchemaProvider.Persist(item); 
        }
        persistenceService.SchemaProvider.EndTransaction();
    }
    
    public void PersistSingle(IFilePersistent item)
    {
        persistenceService.SchemaProvider.BeginTransaction();
        persistenceService.SchemaProvider.Persist(item); 
        persistenceService.SchemaProvider.EndTransaction();
    }
    private void Persist( AbstractSchemaItem item)
    {
        persistenceService.SchemaProvider.Persist(item);
        foreach (var ancestor in item.Ancestors)
        {
            persistenceService.SchemaProvider.Persist(ancestor);
        }
        foreach (var child in item.ChildItems)
        {
            if (child.DerivedFrom == null && child.IsPersistable)
            {
                Persist(child);
            }
        }
    }
}
