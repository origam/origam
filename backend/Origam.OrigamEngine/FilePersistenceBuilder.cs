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
using Origam.DA;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine;

public class FilePersistenceBuilder : IPersistenceBuilder
{
    private static FilePersistenceService persistenceService;

    public IDocumentationService GetDocumentationService() =>
        new FileStorageDocumentationService(
            (IFilePersistenceProvider)persistenceService.SchemaProvider,
            persistenceService.FileEventQueue
        );

    public IPersistenceService GetPersistenceService() =>
        GetPersistenceService(watchFileChanges: true, checkRules: true, useBinFile: true);

    public IPersistenceService GetPersistenceService(
        bool watchFileChanges,
        bool checkRules,
        bool useBinFile
    )
    {
        persistenceService = CreateNewPersistenceService(watchFileChanges, checkRules, useBinFile);
        return persistenceService;
    }

    public FilePersistenceService CreateNewPersistenceService(
        bool watchFileChanges,
        bool checkRules,
        bool useBinFile
    )
    {
        List<string> defaultFolders = new List<string>
        {
            CategoryFactory.Create(typeof(Package)),
            CategoryFactory.Create(typeof(SchemaItemGroup)),
        };
        var metaModelUpgradeService = ServiceManager.Services.GetService<MetaModelUpgradeService>();

#if !ORIGAM_CLIENT
        MetaModelUpgradeMode mode = MetaModelUpgradeMode.Upgrade;
#else
        MetaModelUpgradeMode mode = MetaModelUpgradeMode.Ignore;
#endif
        string pathToRuntimeModelConfig = ConfigurationManager
            .GetActiveConfiguration()
            .PathToRuntimeModelConfig;
        return new FilePersistenceService(
            metaModelUpgradeService: metaModelUpgradeService,
            defaultFolders: defaultFolders,
            pathToRuntimeModelConfig: pathToRuntimeModelConfig,
            watchFileChanges: watchFileChanges,
            useBinFile: useBinFile,
            checkRules: checkRules,
            mode: mode
        );
    }

    public FilePersistenceService CreateNoBinFilePersistenceService()
    {
        List<string> defaultFolders = new List<string>
        {
            CategoryFactory.Create(typeof(Package)),
            CategoryFactory.Create(typeof(SchemaItemGroup)),
        };
        string pathToRuntimeModelConfig = ConfigurationManager
            .GetActiveConfiguration()
            .PathToRuntimeModelConfig;

        return new FilePersistenceService(
            new NullMetaModelUpgradeService(),
            defaultFolders: defaultFolders,
            pathToRuntimeModelConfig: pathToRuntimeModelConfig,
            watchFileChanges: false,
            useBinFile: false,
            mode: MetaModelUpgradeMode.Ignore
        );
    }

    public static void Clear()
    {
        persistenceService = null;
    }
}
