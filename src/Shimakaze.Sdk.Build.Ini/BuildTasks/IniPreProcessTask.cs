using Microsoft.Build.Framework;

using Shimakaze.Sdk.Preprocessor.Ini;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build.Ini.BuildTasks;

public sealed class IniPreProcessTask : MSTask
{
    [Required] public ITaskItem[]? FileList { get; set; }
    [Required] public string? EntryFile { get; set; }
    [Required] public string? BaseDirectory { get; set; }
    [Required] public string? OutputTarget { get; set; }
    public string? Extensions { get; set; }
    public string? Defines { get; set; }

    public override bool Execute()
    {
        PrivateExecute().Wait();
        return true;
    }

    private async Task PrivateExecute()
    {
        await using var output = File.CreateText(OutputTarget!);
        IniPreprocessor inipp = new();
        await inipp.InitializeAsync(
            output,
            BaseDirectory!,
            FileList!.Select(i =>
            {
                if (File.Exists(i.ItemSpec))
                    return new FileInfo(i.ItemSpec);

                if (File.Exists(Path.Combine(BaseDirectory!, i.ItemSpec)))
                    return new FileInfo(Path.Combine(BaseDirectory!, i.ItemSpec));

                throw new FileNotFoundException(i.ItemSpec);
            }),
            Defines?.Split(';') ?? Array.Empty<string>(),
            Extensions?.Split(';') ?? Array.Empty<string>()
        );

        await inipp.ExecuteAsync(new FileInfo(Path.Combine(BaseDirectory!, EntryFile!)));
    }
}