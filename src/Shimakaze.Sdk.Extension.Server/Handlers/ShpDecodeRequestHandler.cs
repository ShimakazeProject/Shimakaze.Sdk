using MediatR;

using OmniSharp.Extensions.JsonRpc;

using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Shimakaze.Sdk.Extension.Server.Handlers;

internal sealed record class ShpDecodeResponseFrame(ShapeFrameHeader Metadata, string? Image);
internal sealed record class ShpDecodeResponse(ShapeFileHeader Metadata, ShpDecodeResponseFrame[] Frames);

internal sealed record class ShpDecodeRequestParams(
    string ShapePath,
    string PalettePath
) : IRequest<ShpDecodeResponse>;

[Method("/shp/decode", Direction.ClientToServer)]
internal sealed partial class ShpDecodeRequestHandler()
    : IJsonRpcRequestHandler<ShpDecodeRequestParams, ShpDecodeResponse>
{
    public async Task<ShpDecodeResponse> Handle(ShpDecodeRequestParams request, CancellationToken cancellationToken)
    {
        Palette palette;
        await using (Stream paletteStream = File.OpenRead(request.PalettePath))
            palette = PaletteReader.Read(paletteStream);

        await using Stream shapeStream = File.OpenRead(request.ShapePath);
        ShapeImage shape = ShapeReader.Read(shapeStream);

        var frames = await Task.WhenAll(shape.Frames.Select(async frame =>
        {
            string? base64 = null;
            if (frame.Width is not 0 && frame.Height is not 0)
            {
                await using MemoryStream ms = new();
                foreach (var index in frame.Indexes)
                    ms.Write(palette[index]);

                using Image<Rgb24> image = Image.LoadPixelData<Rgb24>(
                    ms.ToArray(),
                    frame.Width,
                    frame.Height);

                await using MemoryStream outputStream = new();
                await image.SaveAsWebpAsync(outputStream).ConfigureAwait(false);
                base64 = "data:image/webp;base64," + Convert.ToBase64String(outputStream.ToArray());
            }

            return new ShpDecodeResponseFrame(frame.Metadata, base64);
        }));


        return new(shape.Metadata, frames);
    }
}
