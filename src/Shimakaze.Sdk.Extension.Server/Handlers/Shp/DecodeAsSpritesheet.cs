using MediatR;

using OmniSharp.Extensions.JsonRpc;

using Shimakaze.Sdk.Extension.Server.Models.PIXI;
using Shimakaze.Sdk.Extension.Server.Services.Shp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Shimakaze.Sdk.Extension.Server.Handlers.Shp;

internal sealed record class DecodeAsSpritesheetResponse(SpritesheetData SpritesheetData);

internal sealed record class DecodeAsSpritesheetRequest(
    string Shp,
    string Pal,
    bool HasShadow
) : IRequest<DecodeAsSpritesheetResponse>;


[Method("/shp/decode/spritesheet", Direction.ClientToServer)]
internal sealed partial class DecodeAsSpritesheet()
    : IJsonRpcRequestHandler<DecodeAsSpritesheetRequest, DecodeAsSpritesheetResponse>
{
    public async Task<DecodeAsSpritesheetResponse> Handle(DecodeAsSpritesheetRequest request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out var shp, out var pal, out var hasShadow);
        (SpritesheetData data, Image<Rgb24> image) = await ShpDecoder.GetSpritesheetDataAsync(shp, pal, hasShadow, cancellationToken);
        using var spritesheet = image;

        // ×ª»»Í¼ÏñÎªdata url
        await using MemoryStream outputStream = new();
        await spritesheet.SaveAsWebpAsync(outputStream, cancellationToken: cancellationToken);
        string dataUri = "data:image/webp;base64," + Convert.ToBase64String(outputStream.ToArray());
        data.Meta.Image = dataUri;

        return new(data);
    }
}