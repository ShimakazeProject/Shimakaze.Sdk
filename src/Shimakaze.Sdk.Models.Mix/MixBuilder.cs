namespace Shimakaze.Sdk.Models.Mix;

public class MixBuilder
{
    public static async Task Build(Stream output, FileInfo[] files, TextWriter fileMapWriter, bool legacy = false)
    {
        long startOffset = output.Position;
        await fileMapWriter.WriteLineAsync($"# Shimakaze.Sdk.Models.Mix Map").ConfigureAwait(false);
        if (!legacy)
        {
            await output.WriteAsync(BitConverter.GetBytes(0)).ConfigureAwait(false);
        }

        await output.WriteAsync(BitConverter.GetBytes((short)files.Length)).ConfigureAwait(false);
        await output.WriteAsync(BitConverter.GetBytes(0)).ConfigureAwait(false);

        int bodySize = 0;
        foreach (FileInfo file in files)
        {
            uint id = GetId(file.Name, legacy);
            await fileMapWriter.WriteLineAsync($"0x{id:X8} : {file.Name}").ConfigureAwait(false);
            await output.WriteAsync(BitConverter.GetBytes(id)).ConfigureAwait(false);
            await output.WriteAsync(BitConverter.GetBytes(bodySize)).ConfigureAwait(false);
            bodySize += (int)file.Length;
            await output.WriteAsync(BitConverter.GetBytes(bodySize)).ConfigureAwait(false);
        }
        foreach (FileInfo file in files)
        {
            await using FileStream fs = file.OpenRead();
            await fs.CopyToAsync(output).ConfigureAwait(false);
        }
        long currentOffset = output.Position;
        output.Seek(startOffset + 2, SeekOrigin.Begin);
        await output.WriteAsync(BitConverter.GetBytes(bodySize)).ConfigureAwait(false);
        output.Seek(currentOffset, SeekOrigin.Begin);
    }

    private static uint GetId(string name, bool legacy = false) => legacy
        ? IdCalculaters.LegacyIdCalculater(name)
        : IdCalculaters.IdCalculater(name);

}