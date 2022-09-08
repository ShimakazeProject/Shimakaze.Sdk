using System.Runtime.InteropServices;

using Shimakaze.Sdk.Models.Tmp;

namespace Shimakaze.Sdk.Loader.Tmp;

public sealed class TmpReadOptions : ReadOptions
{
    public static new TmpReadOptions Default { get; } = new();
    public int BufferSize { get; set; } = 1024;
}
public sealed class TmpWriteOptions : WriteOptions
{
    public static new TmpWriteOptions Default { get; } = new();
    public int BufferSize { get; set; } = 1024;
}

public class TmpLoader : Loader<TmpFile, TmpReadOptions, TmpWriteOptions>
{
    public override async Task<TmpFile> ReadAsync(Stream stream, TmpReadOptions? options = null)
    {
        options ??= TmpReadOptions.Default;
        byte[] bytes = new byte[options.BufferSize];
        Memory<byte> buffer;
        TmpFile file = new();

        // Header
        unsafe
        {
            buffer = bytes.AsMemory(0, sizeof(FileHeader)); 
        }
        await stream.ReadAsync(buffer);
        unsafe
        {
            file.Header = *(FileHeader*)buffer.Pin().Pointer;
        }

        // Offset
        buffer = bytes.AsMemory(0, sizeof(uint));
        await stream.ReadAsync(buffer);
        int offset;
        unsafe
        {
            offset = *(int*)buffer.Pin().Pointer;
        }

        // Calc
        uint count = file.Header.BlockWidth * file.Header.BlockHeight;
        file.TileCellHeaders = new TileCellHeader[count];

        // TileCellHeaders
        for (int i = 0; i < count; i++)
        {
            unsafe
            {
                buffer = bytes.AsMemory(0, sizeof(TileCellHeader));
            }
            await stream.ReadAsync(buffer);
            unsafe
            {
                file.TileCellHeaders[i] = *(TileCellHeader*)buffer.Pin().Pointer;
            }
        }


        return file;
    }

    public override async Task WriteAsync(TmpFile document, Stream stream, TmpWriteOptions? options = null)
    {
        options ??= TmpWriteOptions.Default;
        byte[] bytes = new byte[options.BufferSize];
        Memory<byte> buffer;

        // Header
        buffer = bytes.AsMemory(0, sizeof(uint) * 4);
        unsafe
        {
            Buffer.MemoryCopy(
                &document.Header,
                buffer.Pin().Pointer,
                sizeof(FileHeader),
                sizeof(FileHeader));
        }
        await stream.WriteAsync(buffer);

        // Offset
        int offset = (int)stream.Position + sizeof(int);
        buffer = bytes.AsMemory(0, sizeof(int));
        unsafe
        {
            Buffer.MemoryCopy(
                &offset,
                buffer.Pin().Pointer,
                sizeof(int),
                sizeof(int));
        }
        await stream.WriteAsync(buffer);

        // Calc
        uint count = document.Header.BlockWidth * document.Header.BlockHeight;

        // TileCellHeaders

        for (int i = 0; i < count; i++)
        {
            buffer = bytes.AsMemory(0, sizeof(uint) * 13);
            unsafe
            {
                fixed (TileCellHeader* pArr = document.TileCellHeaders)
                    Buffer.MemoryCopy(
                        &pArr[i],
                        buffer.Pin().Pointer,
                        sizeof(TileCellHeader),
                        sizeof(TileCellHeader));
            }
            await stream.WriteAsync(buffer);
        }

    }
}
