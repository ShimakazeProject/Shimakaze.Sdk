using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vpl;
using Shimakaze.Sdk.Vpl.Editor;

using Spectre.Console;

var vplPath = await AnsiConsole.AskAsync<string>("What VPL File do you want edit?");
vplPath = vplPath.Trim('"');
var palPath = await AnsiConsole.AskAsync<string>("What PAL File do you want see?");
palPath = palPath.Trim('"');

VoxelPalette vpl;
Palette pal;

await using (Stream vplStream = File.OpenRead(vplPath))
    vpl = VoxelPalette.ReadFrom(vplStream);

await using (Stream palStream = File.OpenRead(palPath))
    pal = Palette.ReadFrom(palStream);

VplEditor editor = new(vpl, pal, async (editor, cancellationToken) =>
{
    var path = await AnsiConsole.AskAsync("Where is your new VPL file save to?", vplPath, cancellationToken);
    path = path.Trim('"');
    await using Stream fs = File.Create(path);
    editor.Vpl.WriteTo(fs);
});

await editor.RunAsync(default);
