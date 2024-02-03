using MediatR;

using OmniSharp.Extensions.JsonRpc;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Extension.Server.Handlers;

internal sealed record class PalReaderRequest(
    string PaletteFilePath
) : IRequest<Palette>;

[Method("/pal/decode", Direction.ClientToServer)]
internal sealed partial class PalReader
    : IJsonRpcRequestHandler<PalReaderRequest, Palette>
{
    public async Task<Palette> Handle(PalReaderRequest request, CancellationToken cancellationToken)
    {
        await using Stream paletteStream = File.OpenRead(request.PaletteFilePath);
        return PaletteReader.Read(paletteStream);
    }
}
