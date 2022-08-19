using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using lzo.net;

using Microsoft.AspNetCore.Mvc;

namespace Shimakaze.Sdk.Service.GPL.Services;

[Route("/v1/[controller]"), ApiController]
public class IsoMapPack5Controller : Controller
{
    [HttpPost("Decompress")]
    public async Task DecompressAsync()
    {
        using BinaryReader br = new(Request.Body);
        byte[] buffer = new byte[8192];
        while (br.PeekChar() >= 0)
        {
            var blockSize = br.ReadUInt16();
            var outputSize = br.ReadUInt16();
            var lzo = br.ReadBytes(blockSize);

            await using MemoryStream ms = new(lzo);
            using LzoStream stream = new(ms, CompressionMode.Decompress);
            await stream.ReadAsync(buffer.AsMemory(0, outputSize)).ConfigureAwait(false);
            await Response.BodyWriter.WriteAsync(buffer).ConfigureAwait(false);
        }
        await Response.BodyWriter.FlushAsync().ConfigureAwait(false);
    }
}
