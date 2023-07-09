using Sharprompt;

using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.IO.Vpl;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vpl;
using Shimakaze.Sdk.Vpl.Editor;

string vplPath = Prompt.Input<string>("What VPL File do you want edit?").Trim('"');
string palPath = Prompt.Input<string>("What PAL File do you want see?").Trim('"');

VoxelPalette vpl;
Palette pal;

await using (var vplStream = File.OpenRead(vplPath))
await using (VoxelPaletteReader vplReader = new(vplStream))
    vpl = vplReader.Read();

await using (var palStream = File.OpenRead(palPath))
await using (PaletteReader palReader = new(palStream))
    pal = palReader.Read();

VplEditor editor = new(vpl, pal, async (editor) =>
{
    string path = Prompt.Input<string>("Where is your new VPL file save to?", vplPath);
    await using var fs = File.Create(path);
    await using VoxelPaletteWriter writer = new(fs);
    writer.Write(editor.Vpl);
});

await editor.RunAsync();