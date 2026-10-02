using System.Globalization;

namespace RevengeTools.Cli;

public sealed class CommandLine
{
    private readonly Dictionary<string, string?> _options = new(StringComparer.OrdinalIgnoreCase);

    private CommandLine(List<string> positionals)
    {
        Positionals = positionals;
    }

    public IReadOnlyList<string> Positionals { get; }

    public static CommandLine Parse(IEnumerable<string> arguments)
    {
        string[] values = arguments.ToArray();
        var positionals = new List<string>();
        var result = new CommandLine(positionals);
        for (int index = 0; index < values.Length; index++)
        {
            string value = values[index];
            if (!value.StartsWith("--", StringComparison.Ordinal))
            {
                positionals.Add(value);
                continue;
            }

            string name = value[2..];
            if (name.Length == 0 || !result._options.TryAdd(name, null))
                throw new ArgumentException($"Invalid or duplicate option '{value}'.");
            if (index + 1 < values.Length && !values[index + 1].StartsWith("--", StringComparison.Ordinal))
                result._options[name] = values[++index];
        }
        return result;
    }

    public bool Has(string name) => _options.ContainsKey(name);

    public string? Get(string name)
    {
        return _options.TryGetValue(name, out string? value) ? value : null;
    }

    public string RequireValue(string name)
    {
        string? value = Get(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"--{name} requires a value.");
        return value;
    }

    public int ParseNumber(string value)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.Parse(value[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    public void RequireOnly(params string[] names)
    {
        var allowed = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        string? unknown = _options.Keys.FirstOrDefault(name => !allowed.Contains(name));
        if (unknown is not null)
            throw new ArgumentException($"Unknown option '--{unknown}'.");
    }
}
