using System.Globalization;

namespace Shimakaze.Sdk.Engine.Mix;

internal class MixExporter(Stream mix)
{
    private readonly Dictionary<uint, string> _nameMap = [];

    public bool IsTDMode { get; set; }

    public async Task ParseNameMapAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        while (await reader.ReadLineAsync(cancellationToken) is string line)
        {
            // 定位到指定节
            if (line.StartsWith("[NameMap]", StringComparison.Ordinal))
                break;
        }

        while (await reader.ReadLineAsync(cancellationToken) is string line && !line.StartsWith('['))
        {
            string data = line.Split(';', '#')[0];
            string[] kvp = data.Split('=', StringSplitOptions.TrimEntries);
            if (uint.TryParse(kvp[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint id))
                _nameMap[id] = kvp[1];
        }
    }

    public async Task Export(string destinationPath, CancellationToken cancellationToken = default)
    {
        destinationPath = Path.GetFullPath(destinationPath);
        Directory.CreateDirectory(destinationPath);

        var entries = Sdk.Mix.Mix.ReadMetadata(mix, out _, out _, out int bodyOffset, IsTDMode);

        for (int i = 0; i < entries.Length; i++)
        {
            string name = _nameMap.TryGetValue(entries[i].Id, out string? value)
                ? value
                : $"{entries[i].Id:X8}";

            using var output = File.Create(Path.Combine(destinationPath, name));
            await Sdk.Mix.Mix.ReadFileAsync(mix, bodyOffset, entries[i], output, cancellationToken: cancellationToken);
        }
    }
}
