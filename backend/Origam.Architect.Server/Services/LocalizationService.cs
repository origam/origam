using Origam.DA.ObjectPersistence;
using Origam.OrigamEngine;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class LocalizationService(SchemaService schemaService)
{
    private readonly object _rebuildLock = new();

    public IReadOnlyList<string> Rebuild()
    {
        lock (_rebuildLock)
        {
            var package = schemaService.ActiveExtension;
            if (package == null)
            {
                throw new UserOrigamException(Strings.Localization_NoActivePackage);
            }

            var settings = ConfigurationManager.GetActiveConfiguration();
            var languages = (settings.TranslationBuilderLanguages ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();
            if (languages.Length == 0)
            {
                throw new UserOrigamException(Strings.Localization_NoLanguages);
            }

            string outputFolder = string.IsNullOrEmpty(settings.LocalizationFolder)
                ? Path.GetFullPath(Path.Combine(settings.ModelSourceControlLocation, "..", "l10n"))
                : settings.LocalizationFolder;
            Directory.CreateDirectory(outputFolder);

            var files = new List<string>();
            foreach (string language in languages)
            {
                string fileName = package.Name + "-" + language + ".xml";
                string outputPath = Path.Combine(outputFolder, fileName);
                using var currentTranslations = File.Exists(outputPath)
                    ? new LocalizationCache(outputPath)
                    : null;
                using var output = new MemoryStream();
                TranslationBuilder.Build(output, currentTranslations, language, package.Id);
                using var file = new FileStream(outputPath, FileMode.Create);
                output.Position = 0;
                output.CopyTo(file);
                files.Add(fileName);
            }
            return files;
        }
    }
}
