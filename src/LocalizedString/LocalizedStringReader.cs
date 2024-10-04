using Microsoft.Extensions.Logging;

namespace LocalizedString;

public interface ILocalizedStringReader
{
    string this[string name] { get; }
    string this[string name, params object[] arguments] { get; }
    IEnumerable<string> GetAllStrings(bool includeParentCultures);
}

public interface ILocalizedStringReader<TResourceSource>
{
    string this[string name] { get; }
    string this[string name, params object[] arguments] { get; }
    IEnumerable<string> GetAllStrings(bool includeParentCultures);
}

public class LocalizedStringReader<TResourceSource> : ILocalizedStringReader<TResourceSource>, ILocalizedStringReader
{
    private readonly ILogger<LocalizedStringReader<TResourceSource>> _logger;
    private readonly ILocalizedStringReader _localizer;
    //
    // Summary:
    //     Creates a new Microsoft.Extensions.Localization.StringLocalizer`1.
    //
    // Parameters:
    //   factory:
    //     The Microsoft.Extensions.Localization.IStringLocalizerFactory to use.
    public LocalizedStringReader(
        ILocalizedStringFactory factory,
        ILogger<LocalizedStringReader<TResourceSource>> logger)
    {
        _logger = logger;
        _localizer = (ILocalizedStringReader)factory.Create(typeof(TResourceSource)).Result;
    }

    public virtual string this[string name]
    {
        get
        {
            _logger.LogDebug("Getting string for {name}", name);
            return _localizer[name];
        }
    }
    public virtual string this[string name, params object[] arguments]
    {
        get
        {
            _logger.LogDebug("Getting string for {name} with {arguments}", name, arguments);
            return _localizer[name, arguments];
        }
    }

    public IEnumerable<string> GetAllStrings(bool includeParentCultures = false)
    {
        return _localizer.GetAllStrings(includeParentCultures);
    }
}


public class LocalizedStringReader : ILocalizedStringReader
{
    private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();
    public LocalizedStringReader(Dictionary<string, string> _dictionary)
    {
        this._dictionary = _dictionary;
    }

    public virtual string this[string name]
    {
        get
        {
            if (_dictionary.ContainsKey(name))
            {
                return _dictionary[name];
            }
            return name;
        }
    }
    public virtual string this[string name, params object[] arguments]
    {
        get
        {
            if (_dictionary.ContainsKey(name))
            {
                return string.Format(_dictionary[name], arguments);
            }
            return name;
        }
    }

    public IEnumerable<string> GetAllStrings(bool includeParentCultures)
    {
        return _dictionary.Keys;
    }
}