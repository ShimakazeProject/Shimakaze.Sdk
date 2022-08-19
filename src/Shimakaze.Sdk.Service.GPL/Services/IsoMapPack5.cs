using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using lzo.net;

using Shimakaze.Sdk.Service.Web.Servlet;

namespace Shimakaze.Sdk.Service.GPL.Services;

/// <summary>
/// IsoMapPack5 Decompress
/// </summary>
[WebServlet("/v1/IsoMapPack5/Decompress", "Decompress IsoMapPack5 Block")]
public class IsoMapPack5Decompresser : HttpServlet
{
    /// <summary>
    /// IsoMapPack5 Decompress
    /// </summary>
    protected override void OnPost(HttpListenerRequest request, HttpListenerResponse response)
    {
        using BinaryReader br = new(request.InputStream);
        byte[] buffer = new byte[8192];
        while (br.PeekChar() >= 0)
        {
            var blockSize = br.ReadUInt16();
            var outputSize = br.ReadUInt16();
            var lzo = br.ReadBytes(blockSize);

            using MemoryStream ms = new(lzo);
            using LzoStream stream = new(ms, CompressionMode.Decompress);
            stream.Read(buffer, 0, outputSize);
            response.OutputStream.Write(buffer);
        }
        response.OutputStream.Flush();
        response.StatusCode = (int)HttpStatusCode.OK;
        response.Close();
    }
}
