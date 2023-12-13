using Sharprompt;

using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vpl;
using Shimakaze.Sdk.Vpl.Editor;

string vplPath = Prompt.Input<string>("What VPL File do you want edit?").Trim('"');
string palPath = Prompt.Input<string>("What PAL File do you want see?").Trim('"');

VoxelPalette vpl;
Palette pal;

using (Stream vplStream = File.OpenRead(vplPath))
await using (VoxelPaletteReader vplReader = new(vplStream))
    vpl = await vplReader.ReadAsync();

using (Stream palStream = File.OpenRead(palPath))
await using (PaletteReader palReader = new(palStream))
    pal = await palReader.ReadAsync();

VplEditor editor = new(vpl, pal, async (editor) =>
{
    string path = Prompt.Input<string>("Where is your new VPL file save to?", vplPath);
    using Stream fs = File.Create(path);
    await using VoxelPaletteWriter writer = new(fs);
    await writer.WriteAsync(editor.Vpl);
});

await editor.RunAsync();