using Microsoft.Extensions.Logging;

namespace LocalizedString;

public interface ILocalizedStringFactory
{
    Task<ILocalizedStringReader> Create(Type resourceSource);
    Task<ILocalizedStringReader> Create(string baseName);
}

/// <summary>
/// Creates an IAppStringLocalizer
/// s-reads the resources from the file system
/// 
/// </summary>
public class LocalizedStringFactory(
        ILocalizedStringResources resources, 
        ILogger<LocalizedStringFactory> logger) : ILocalizedStringFactory
{
    //
    // Summary:
    //     Creates an Microsoft.Extensions.Localization.IStringLocalizer using the System.Reflection.Assembly
    //     and System.Type.FullName of the specified System.Type.
    //
    // Parameters:
    //   resourceSource:
    //     The System.Type.
    //
    // Returns:
    //     The Microsoft.Extensions.Localization.IStringLocalizer.
    public Task<ILocalizedStringReader> Create(Type resourceSource)
    {
        return Create(resourceSource.FullName!);
    }

    //
    // Summary:
    //     Creates an IAppStringLocalizer.
    //
    // Parameters:
    //   baseName:
    //     The base name of the resource to load strings from.
    //
    //   location:
    //     The location to load resources from.
    //
    // Returns:
    //     The IAppStringLocalizer.
    public Task<ILocalizedStringReader> Create(string baseName)
    {
        logger.LogDebug("Creating AppStringLocalizer for {baseName}", baseName);
        Dictionary<string, string> dictionary = resources.Read(baseName);
        logger.LogDebug("AppStringLocalizer for {baseName} created with {dictionaryCount} entries", baseName, dictionary?.Count ?? 0);
        return Task.FromResult<ILocalizedStringReader>(new LocalizedStringReader(dictionary ?? default!));
    }

}