using Microsoft.Extensions.Options;

namespace PostKit.Configuration;

internal sealed class RebasedOptions<TOptions>(IOptionsMonitor<TOptions> monitor) : IOptions<TOptions>
    where TOptions : class
{
    public TOptions Value => monitor.CurrentValue;
}

internal sealed class RebasedOptionsSnapshot<TOptions>(IOptionsFactory<TOptions> factory, string defaultName) : IOptionsSnapshot<TOptions>
    where TOptions : class
{
    private readonly OptionsCache<TOptions> _cache = new();
    public TOptions Value => Get(Options.DefaultName);

    public TOptions Get(string? name)
    {
        var effectiveName = MapName(name);
        return _cache.GetOrAdd(effectiveName, () => factory.Create(effectiveName));
    }

    private string MapName(string? name)
    {
        return string.IsNullOrEmpty(name) ? defaultName : name;
    }
}

internal sealed class RebasedOptionsMonitor<TOptions>(IOptionsFactory<TOptions> factory, IEnumerable<IOptionsChangeTokenSource<TOptions>> sources, IOptionsMonitorCache<TOptions> cache, string defaultName)
    : IOptionsMonitor<TOptions>, IDisposable
    where TOptions : class
{
    private readonly OptionsMonitor<TOptions> _inner = new(factory, sources, cache);

    public void Dispose()
    {
        _inner.Dispose();
    }

    public TOptions CurrentValue => Get(Options.DefaultName);

    public TOptions Get(string? name)
    {
        return _inner.Get(MapName(name));
    }

    public IDisposable OnChange(Action<TOptions, string?> listener)
    {
        return _inner.OnChange((options, name) => listener(options, UnmapName(name)));
    }

    private string MapName(string? name)
    {
        return string.IsNullOrEmpty(name) ? defaultName : name;
    }

    private string? UnmapName(string? name)
    {
        return string.Equals(name, defaultName, StringComparison.Ordinal) ? Options.DefaultName : name;
    }
}