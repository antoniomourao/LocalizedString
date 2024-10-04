using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LocalizedString;

public interface ILocalizedStringResources
{
    Dictionary<string, string> Read(string baseName);
    Dictionary<string, string> Write(string baseName, Dictionary<string, string> content);
}

public class LocalizedStringResources : ILocalizedStringResources
{
    private readonly ILogger<LocalizedStringResources> _logger;
    private readonly ILocalizedStringOptions _localizerOptions;
    private string _resourcesPath => _localizerOptions.ResourcesPath;
    private string _currentCultureName;

    /// <summary>
    /// Gets the default culture name
    /// Default culture name is the first supported culture
    /// </summary>
    private string _defaultCultureName => _localizerOptions.SupportedCultures[0];

    public LocalizedStringResources(
        ILocalizedStringOptions localizerOptions,
        ILogger<LocalizedStringResources> logger)
    {
        _logger = logger;
        _localizerOptions = localizerOptions;
        _currentCultureName = getValidCultureName(CultureInfo.CurrentCulture.Name);
    }

    public void SetCultureName(string cultureName)
    {
        _currentCultureName = getValidCultureName(cultureName);
        _logger.LogDebug("Setting culture name to {cultureName}. (Culture name set to {_cultureName})", cultureName, _currentCultureName);
    }

    public string GetCurrentCultureName { get { return _currentCultureName; } }

    /// <summary>
    /// Writes the resources to the file system
    /// </summary>
    /// <param name="baseName"></param>
    /// <param name="location"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public Dictionary<string, string> Write(string baseName, Dictionary<string, string> content)
    {
        _logger.LogDebug("Write resources for {baseName} using {resourcesPath}", baseName, _resourcesPath);
        var filePath = Path.Combine(_resourcesPath, baseName);
        Directory.CreateDirectory(_resourcesPath);
        var fileName = getFileName(filePath, _currentCultureName);
        if (File.Exists(fileName))
        {
            _logger.LogInformation("Deleting file {fileName}", fileName);
            File.Delete(fileName);
        }
        var resourceContent = processResourceTypeContent(content);

        _logger.LogDebug("Writing file {fileName} with serialized content", fileName);
        File.WriteAllText(fileName, resourceContent);
        return content;
    }

    internal string processResourceTypeContent(Dictionary<string, string> content)
    {
        if (_localizerOptions.ResourcesExtension == "json")
        {
            return JsonSerializer.Serialize(content);
        }
        else
        {
            return string.Join(Environment.NewLine, content.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
    }
    /// <summary>
    /// Reads the resources from the file system
    /// -tries to find the file with the locale name
    /// -tries to find the file with the first part of the locale name
    /// -tries to find the file without the locale name
    /// -returns an empty dictionary if no file is found
    /// </summary>
    /// <param name="baseName"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public Dictionary<string, string> Read(string baseName)
    {
        _logger.LogDebug("Read resources for {baseName} using {resourcesPath}", baseName, _resourcesPath);
        var dictionary = new Dictionary<string, string>();
        var filePath = Path.Combine(_resourcesPath, baseName);
        var fileName = getResourceFileName(filePath);
        if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
        {
            _logger.LogDebug("Reading file {fileName}", fileName);
            dictionary = readResourceTypeContent(fileName);
        }
        return dictionary;
    }

    internal Dictionary<string, string> readResourceTypeContent(string fileName)
    {
        var dictionary = new Dictionary<string, string>();

        if (_localizerOptions.ResourcesExtension == "json")
        {
            var jsonString = File.ReadAllText(fileName);
            dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
        }
        else
        {
            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length >= 2)
                {
                    dictionary.Add(parts[0], string.Join("=", parts[1..]));
                }
            }
        }

        return dictionary ?? default!;
    }

    /// <summary>
    /// Gets the resource file name using the locale name.
    /// -first tries with the first part of the locale name (en-US => en)
    /// -then tries with the full locale name (en-US)
    /// -then tries without the locale name (en-US => en-US)
    /// -finally returns null if no file is found
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    internal string? getResourceFileName(string filePath)
    {
        var cultureList = new List<string> { _currentCultureName.Split('-')[0], _currentCultureName, string.Empty };
        foreach (var culture in cultureList)
        {
            var fileName = getFileName(filePath, culture);
            if (File.Exists(fileName))
            {
                return fileName;
            }
            _logger.LogDebug("Not founf! File name {fileName}", fileName);
        }
        _logger.LogInformation("Returning null! Location not found in {filePath}", filePath);
        return null;
    }

    /// <summary>
    /// Gets the file name with the culture part
    /// -if the culture part is empty, returns the base name
    /// -otherwise returns the base name with the culture part
    /// </summary>
    /// <param name="baseName"></param>
    /// <param name="culturePart"></param>
    /// <returns></returns>
    private string getFileName(string baseName, string culturePart)
    {
        if (culturePart == string.Empty
        || culturePart == _defaultCultureName
        || culturePart == _defaultCultureName.Split('-')[0])
        {
            return $"{baseName}.{_localizerOptions.ResourcesExtension}"; ;
        }
        return $"{baseName}-{culturePart}.{_localizerOptions.ResourcesExtension}";
    }

    /// <summary>
    /// Checks if the culture name is supported
    /// -if the culture name is empty, returns the first supported culture
    /// -if the culture name is supported, returns the culture name
    /// -if the culture name is not supported, returns the first supported culture with the same first part
    /// </summary>
    /// <param name="cultureName"></param>
    /// <returns></returns>
    internal string getValidCultureName(string cultureName)
    {
        if (string.IsNullOrEmpty(cultureName))
        {
            return _defaultCultureName;
        }

        if (_localizerOptions.SupportedCultures.Contains(cultureName))
        {
            return cultureName == _defaultCultureName ? _defaultCultureName : cultureName;
        }

        var supportedCulture = _localizerOptions.SupportedCultures
        .Where(x =>
            x == cultureName
            || x.Split('-')[0] == cultureName.Split('-')[0])
        .Select(x => x)
        .FirstOrDefault();

        if (supportedCulture is null)
        {
            return _defaultCultureName;
        }

        return supportedCulture.ToString();
    }
}