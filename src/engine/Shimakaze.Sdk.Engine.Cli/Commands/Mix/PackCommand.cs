using System.Text;

using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Mix;
using Shimakaze.Sdk.Mix.Structs;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Mix;

[CliCommand(Description = nameof(Resource.Command_Mix_Pack_Description), Parent = typeof(MixCommand))]
internal sealed class PackCommand
{
    [CliOption(Description = nameof(Resource.Command_Mix_Pack_Input_Description), Arity = CliArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = true)]
    public required List<FileInfo> Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Pack_Output_Description))]
    public required FileInfo Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Pack_NameMapOutput_Description))]
    public FileInfo? NameMapOutput { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Pack_IsTDMode_Description))]
    public bool IsTDMode { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Pack_Encrypt_Description))]
    public bool Encrypt { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Pack_Encoding_Description), Required = false)]
    public string Encoding { get; set; } = "ANSI";

    public void Run()
    {
        System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding;
        if ("ANSI".Equals(Encoding, StringComparison.OrdinalIgnoreCase))
            encoding = System.Text.Encoding.GetEncoding(0);
        else if (int.TryParse(Encoding, out int codepage))
            encoding = System.Text.Encoding.GetEncoding(codepage);
        else
            encoding = System.Text.Encoding.GetEncoding(Encoding);

        IdCalculator idCalc = IsTDMode ? IdCalculators.TDIdCalculator : IdCalculators.TSIdCalculator;
        using var archive = MixArchive.Create(idCalc, encoding);

        foreach (var file in Input)
        {
            var entry = archive.CreateEntry(file.Name);
            using var entryStream = entry.Open();
            using var fileStream = file.OpenRead();
            fileStream.CopyTo(entryStream);
        }

        using var output = Output.Create();
        MixTag? flag = Encrypt ? MixTag.ENCRYPTED : MixTag.NONE;
        archive.SaveTo(output, flag);

        if (NameMapOutput is not null)
        {
            using var writer = NameMapOutput.CreateText();
            foreach (var entry in archive.Entries)
            {
                if (entry.Name is not null)
                    writer.WriteLine($"{entry.Id:X8}\t{entry.Name}");
            }
        }
    }
}
