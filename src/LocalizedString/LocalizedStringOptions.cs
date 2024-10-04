namespace LocalizedString;

public interface ILocalizedStringOptions {
    string ResourcesPath { get; set; }
    string ResourcesDirectory { get; set; }
    string ResourcesExtension { get; set; }
    string[] SupportedCultures { get; set; }
} 

public class LocalizedStringOptions: ILocalizedStringOptions
{
    /// <summary>
    /// Path where the resources are located
    /// </summary>
    public string ResourcesPath { get; set; } = string.Empty;

    /// <summary>
    /// Directory where the resources are located
    /// </summary>
    public string ResourcesDirectory { get; set; } = "Resources";

    /// <summary>
    /// Extension of the resources files
    /// </summary>
    public string ResourcesExtension { get; set; } = "strings";

    /// <summary>
    /// Supported cultures
    /// First entry is used as default language
    /// </summary>
    public string[] SupportedCultures { get; set; } = new[] { "en-US", "pt-PT" };
}
