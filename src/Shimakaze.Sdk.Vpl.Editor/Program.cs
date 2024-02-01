using Sharprompt;

using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vpl;
using Shimakaze.Sdk.Vpl.Editor;

string vplPath = Prompt.Input<string>("What VPL File do you want edit?").Trim('"');
string palPath = Prompt.Input<string>("What PAL File do you want see?").Trim('"');

VoxelPalette vpl;
Palette pal;

using (Stream vplStream = File.OpenRead(vplPath))
    vpl = VoxelPaletteReader.Read(vplStream);

using (Stream palStream = File.OpenRead(palPath))
    pal = PaletteReader.Read(palStream);

VplEditor editor = new(vpl, pal, (editor) => {
    string path = Prompt.Input<string>("Where is your new VPL file save to?", vplPath);
    using Stream fs = File.Create(path);
    VoxelPaletteWriter.Write(editor.Vpl, fs);
});

editor.Run();