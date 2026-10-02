using System.Globalization;

namespace CrescentHawksTools.Cli;

public sealed class CommandLine
{
    private readonly Dictionary<string, string?> _options = new(StringComparer.OrdinalIgnoreCase);

    private CommandLine(List<string> positionals)
    {
        Positionals = positionals;
    }

    public IReadOnlyList<string> Positionals { get; }

    public static CommandLine Parse(IEnumerable<string> arguments, params string[] valueOptions)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var expectsValue = new HashSet<string>(valueOptions, StringComparer.OrdinalIgnoreCase);
        string[] values = arguments.ToArray();
        var positionals = new List<string>();
        var result = new CommandLine(positionals);
        bool optionsEnded = false;

        for (int index = 0; index < values.Length; index++)
        {
            string value = values[index];
            if (!optionsEnded && value == "--")
            {
                optionsEnded = true;
                continue;
            }

            if (optionsEnded || !value.StartsWith("--", StringComparison.Ordinal))
            {
                positionals.Add(value);
                continue;
            }

            int equals = value.IndexOf('=');
            string name = equals >= 0 ? value[2..equals] : value[2..];
            string? inlineValue = equals >= 0 ? value[(equals + 1)..] : null;
            if (name.Length == 0 || !result._options.TryAdd(name, null))
                throw new ArgumentException($"Invalid or duplicate option '{value}'.");

            if (expectsValue.Contains(name))
            {
                string? optionValue = inlineValue;
                if (optionValue is null)
                {
                    if (++index >= values.Length || values[index].StartsWith("--", StringComparison.Ordinal))
                        throw new ArgumentException($"--{name} requires a value.");
                    optionValue = values[index];
                }
                if (optionValue.Length == 0)
                    throw new ArgumentException($"--{name} requires a value.");
                result._options[name] = optionValue;
            }
            else if (inlineValue is not null)
            {
                throw new ArgumentException($"--{name} does not accept a value.");
            }
        }

        return result;
    }

    public bool Has(string name) => _options.ContainsKey(name);

    public string? Get(string name) =>
        _options.TryGetValue(name, out string? value) ? value : null;

    public string RequireValue(string name)
    {
        string? value = Get(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"--{name} requires a value.");
        return value;
    }

    public string RequirePositional(int index, string message) =>
        index < Positionals.Count ? Positionals[index] : throw new ArgumentException(message);

    public void RequirePositionalCount(int minimum, int maximum, string usage)
    {
        if (Positionals.Count < minimum || Positionals.Count > maximum)
            throw new ArgumentException($"Usage: {usage}");
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
