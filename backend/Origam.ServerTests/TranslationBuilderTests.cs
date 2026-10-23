using NUnit.Framework;
using Origam.OrigamEngine;

namespace Origam.ServerTests;

[TestFixture]
public class TranslationBuilderTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" \t\r\n ")]
    [TestCase(", ,\t,\r\n,")]
    public void MissingLanguagesProduceNoLocales(string languages)
    {
        Assert.That(TranslationBuilder.ParseLanguages(languages), Is.Empty);
    }

    [TestCase("cs-CZ,de-DE")]
    [TestCase(" cs-CZ , de-DE ")]
    [TestCase(", cs-CZ ,,\tde-DE,\r\n")]
    public void TrimsLocalesAndIgnoresEmptyEntries(string languages)
    {
        Assert.That(
            TranslationBuilder.ParseLanguages(languages),
            Is.EqualTo(new[] { "cs-CZ", "de-DE" })
        );
    }
}
