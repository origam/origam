#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.Models.Requests;
using Origam.Architect.Server.Models.Responses.ModelCheck;
using Origam.DA.Service;
using Origam.DA.Service.FileSystemModeCheckers;
using Origam.OrigamEngine;
using Origam.Rule;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class ModelCheckService(
    SearchService searchService,
    SchemaService schemaService,
    ILogger<ModelCheckService> logger
)
{
    private static readonly string[] IgnoredDirectories = [".git", "l10n"];

    private readonly SemaphoreSlim runLock = new(initialCount: 1, maxCount: 1);
    private ModelCheckResultModel lastResult = new();

    public ModelCheckResultModel LastResult => lastResult;

    public ModelCheckResultModel Run(CancellationToken cancellationToken)
    {
        if (schemaService.ActiveExtension == null)
        {
            throw new UserOrigamException(Strings.ModelCheck_NoActivePackage);
        }
        if (!runLock.Wait(millisecondsTimeout: 0))
        {
            throw new UserOrigamException(Strings.ModelCheck_AlreadyRunning);
        }
        try
        {
            lastResult = RunChecks(cancellationToken);
            return lastResult;
        }
        catch (AggregateException exception) when (IsCancellation(exception))
        {
            // Parallel queries wrap cancellation in AggregateException.
            throw new OperationCanceledException(
                message: exception.Message,
                innerException: exception,
                token: cancellationToken
            );
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(exception, $"The model check failed: {exception.Message}");
            }
            throw new UserOrigamException(
                string.Format(Strings.ModelCheck_Failed, exception.Message)
            );
        }
        finally
        {
            runLock.Release();
        }
    }

    public void ClearCache()
    {
        lastResult = new ModelCheckResultModel();
    }

    private ModelCheckResultModel RunChecks(CancellationToken cancellationToken)
    {
        // Independent copy: ModelRules.GetErrors mutates RootProvider on every item.
        using FilePersistenceService independentPersistenceService =
            new FilePersistenceBuilder().CreateNoBinFilePersistenceService();
        List<Dictionary<ISchemaItem, string>> errorFragments = ModelRules.GetErrors(
            schemaProviders: new OrigamProviderBuilder()
                .SetSchemaProvider(independentPersistenceService.SchemaProvider)
                .GetAll(),
            independentPersistenceService: independentPersistenceService,
            cancellationToken: cancellationToken
        );
        var persistenceProvider = (FilePersistenceProvider)
            independentPersistenceService.SchemaProvider;
        List<ModelErrorSection> errorSections = persistenceProvider.GetFileErrors(
            ignoreDirectoryNames: IgnoredDirectories,
            cancellationToken: cancellationToken
        );
        // Map before disposal - SearchResult reads via the item's own provider.
        return new ModelCheckResultModel
        {
            LastRunAt = DateTime.Now,
            RuleErrors = MapRuleErrors(errorFragments),
            FileErrorSections = MapFileErrorSections(errorSections),
        };
    }

    private static bool IsCancellation(AggregateException exception)
    {
        return exception
            .Flatten()
            .InnerExceptions.All(inner => inner is OperationCanceledException);
    }

    private List<ModelRuleError> MapRuleErrors(List<Dictionary<ISchemaItem, string>> errorFragments)
    {
        List<Guid> referencePackages = searchService.GetReferencePackages();
        var ruleErrors = new List<ModelRuleError>();
        foreach (Dictionary<ISchemaItem, string> errorFragment in errorFragments)
        {
            foreach (KeyValuePair<ISchemaItem, string> entry in errorFragment)
            {
                if (entry.Key == null)
                {
                    continue;
                }
                SearchResult item = BuildItem(entry.Key, referencePackages);
                if (item == null)
                {
                    continue;
                }
                ruleErrors.Add(new ModelRuleError { Item = item, Message = entry.Value });
            }
        }
        return ruleErrors;
    }

    private SearchResult BuildItem(ISchemaItem schemaItem, List<Guid> referencePackages)
    {
        try
        {
            return searchService.BuildResult(schemaItem, referencePackages);
        }
        catch (Exception ex)
        {
            // A single unreadable item must not lose the whole run.
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    ex,
                    $"Could not build a model check result for schema item {schemaItem.Id}"
                );
            }
            return null;
        }
    }

    private static List<ModelFileErrorSection> MapFileErrorSections(
        List<ModelErrorSection> errorSections
    )
    {
        return errorSections
            .Select(section => new ModelFileErrorSection
            {
                Caption = section.Caption,
                Errors = section
                    .ErrorMessages.Select(message => new ModelFileError
                    {
                        Text = message.Text,
                        Link = message.Link,
                    })
                    .ToList(),
            })
            .ToList();
    }
}
