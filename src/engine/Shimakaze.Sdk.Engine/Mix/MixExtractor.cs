using System.Globalization;

namespace Shimakaze.Sdk.Engine.Mix;

internal static class MixExtractor
{
    public static async Task<Dictionary<uint, string>> ParseNameMapAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        Dictionary<uint, string> nameMap = [];

        while (await reader.ReadLineAsync(cancellationToken) is string line)
        {
            // 定位到指定节
            if (line.StartsWith("[NameMap]", StringComparison.Ordinal))
                break;
        }

        while (await reader.ReadLineAsync(cancellationToken) is string line && !line.StartsWith('['))
        {
            var data = line.Split(';', '#')[0];
            var kvp = data.Split('=', StringSplitOptions.TrimEntries);
            if (uint.TryParse(kvp[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var id))
                nameMap[id] = kvp[1];
        }

        return nameMap;
    }

    public static async Task Extract(Stream mix, string destinationPath, Dictionary<uint, string>? nameMap = null, bool isTDMode = false, CancellationToken cancellationToken = default)
    {
        nameMap ??= [];
        destinationPath = Path.GetFullPath(destinationPath);
        Directory.CreateDirectory(destinationPath);

        var entries = Sdk.Mix.Mix.ReadMetadata(mix, out _, out _, out var bodyOffset, isTDMode);

        for (int i = 0; i < entries.Length; i++)
        {
            string name = nameMap.TryGetValue(entries[i].Id, out string? value)
                ? value
                : $"{entries[i].Id:X8}";

            using var output = File.Create(Path.Combine(destinationPath, name));
            await Sdk.Mix.Mix.ReadFileAsync(mix, bodyOffset, entries[i], output, cancellationToken: cancellationToken);
        }
    }
}
