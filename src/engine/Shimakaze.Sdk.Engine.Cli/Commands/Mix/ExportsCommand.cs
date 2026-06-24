using System.Globalization;

using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Mix;

[CliCommand(Description = nameof(Resource.Command_Mix_Exports_Description), Alias = "x", Parent = typeof(MixCommand))]
internal sealed class ExportsCommand
{
    [CliOption(Description = nameof(Resource.Command_Mix_Exports_Input_Description))]
    public required FileInfo Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Exports_Output_Description))]
    public required DirectoryInfo Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Exports_NameMap_Description))]
    public FileInfo? NameMap { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Exports_IsTDMode_Description))]
    public bool IsTDMode { get; set; }

    public async Task RunAsync()
    {
        IdCalculator? idCalc = IsTDMode ? IdCalculators.TDIdCalculator : null;

        await using var stream = Input.OpenRead();
        using var archive = MixArchive.Open(stream, idCalc, leaveOpen: true);

        Dictionary<uint, string>? nameMap = null;
        if (NameMap is { Exists: true })
        {
            nameMap = [];
            using var reader = NameMap.OpenText();
            while (await reader.ReadLineAsync() is { } line)
            {
                int sep = line.IndexOf('\t');
                if (sep < 0)
                    continue;

                ReadOnlySpan<char> idPart = line.AsSpan(0, sep);
                if (uint.TryParse(idPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint id))
                    nameMap[id] = line[(sep + 1)..];
            }
        }

        foreach (var entry in archive.Entries)
        {
            string? mappedName = null;
            nameMap?.TryGetValue(entry.Id, out mappedName);

            string fileName = entry.Name
                ?? mappedName
                ?? entry.Id.ToString("X8", CultureInfo.InvariantCulture);

            string path = Path.Combine(Output.FullName, fileName);
            string? dir = Path.GetDirectoryName(path);
            if (dir is not null)
                Directory.CreateDirectory(dir);

            using var source = entry.Open();
            await using var dest = File.Create(path);
            await source.CopyToAsync(dest);
        }
    }
}
