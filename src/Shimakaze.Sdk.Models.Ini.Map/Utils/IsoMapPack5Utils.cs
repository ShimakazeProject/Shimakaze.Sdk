using System.IO.Compression;

using lzo.net;


namespace Shimakaze.Sdk.Models.Ini.Map.Utils;

/// <summary>
/// IsoMapPack5 Utils
/// </summary>
public static class LzoUtiles
{
    public static void Decompress(Stream input, Stream output)
    {
        using BinaryReader br = new(input);
        byte[] buffer = new byte[8192];
        while (br.PeekChar() >= 0)
        {
            var blockSize = br.ReadUInt16();
            var outputSize = br.ReadUInt16();
            var lzo = br.ReadBytes(blockSize);

            using MemoryStream ms = new(lzo);
            using LzoStream stream = new(ms, CompressionMode.Decompress);
            stream.Read(buffer, 0, outputSize);
            output.Write(buffer);
        }
        output.Flush();
    }
}
