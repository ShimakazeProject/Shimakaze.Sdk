using Shimakaze.Sdk.Service.Web;
using Shimakaze.Sdk.Utils;

namespace Shimakaze.Sdk.Service.GPL;

static class Program
{
    /// <summary>
    /// Start CSF Language Server
    /// </summary>
    /// <param name="host">Listen Host</param>
    /// <param name="port">Web Server Port</param>
    /// <param name="ssl">Use SSL/TLS</param>
    /// <param name="noColorOutput">Disable Colorize Output</param>
    static async Task Main(string host = "localhost", ushort port = 45082, bool ssl = false, bool noColorOutput = false)
    {
        Logger.UseColor = !noColorOutput;
        await new ServletServer(host, port, ssl).Run();
    }
}