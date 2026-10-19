using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace TOEICReading.Localization;

public static class TOEICReadingLocalizationConfigurer
{
    public static void Configure(ILocalizationConfiguration localizationConfiguration)
    {
        localizationConfiguration.Sources.Add(
            new DictionaryBasedLocalizationSource(TOEICReadingConsts.LocalizationSourceName,
                new XmlEmbeddedFileLocalizationDictionaryProvider(
                    typeof(TOEICReadingLocalizationConfigurer).GetAssembly(),
                    "TOEICReading.Localization.SourceFiles"
                )
            )
        );
    }
}
