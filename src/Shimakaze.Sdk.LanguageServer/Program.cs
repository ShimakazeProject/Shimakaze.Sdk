using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;

using DotMake.CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Server;

using Shimakaze.Sdk.LanguageServer.Handlers;
using Shimakaze.Sdk.LanguageServer.Services;

namespace Shimakaze.Sdk.LanguageServer;

[CliCommand(ShortFormAutoGenerate = false, Description = "为 《Command & Conquer: Red Alert 2》 模组开发提供语言服务")]
internal sealed class Program
{
    #region 命令行参数
    /// <summary>
    /// 使用标准流
    /// </summary>
    [CliOption(Required = false, Description = "使用基本输入输出流进行进程间通信")]
    public bool? Stdio { get; set; }

    /// <summary>
    /// 使用命名管道 / Unix 域套接字 (UDS)
    /// </summary>
    [CliOption(Required = false, Description = "使用命名管道(For Windows)/Unix 域套接字(for Linux)进行进程间通信")]
    public string? Pipe { get; set; }

    /// <summary>
    /// 使用网络套接字
    /// </summary>
    [CliOption(Required = false, HelpName = "port", Description = "使用TCP流进行进程间通信")]
    public ushort? Socket { get; set; }

    /// <summary>
    /// 客户端进程ID
    /// </summary>
    [CliOption(Required = false, Name = "--clientProcessId", HelpName = "pid", Description = "客户端的进程ID，监听编辑器是否退出")]
    public int? ClientProcessId { get; set; }
    #endregion

    #region 命令行参数解析
    private static Task<int> Main(string[] args) => Cli.RunAsync<Program>(args);

    internal async Task RunAsync()
    {

        if (Stdio is true)
            await RunStdioAsync();
        else if (!string.IsNullOrWhiteSpace(Pipe))
            await RunPipeAsync(Pipe);
        else if (Socket.HasValue)
            await RunSocketAsync(Socket.Value);
        else
            await Main(["--help"]);
    }

    private async Task RunStdioAsync()
    {
        await using Stream input = Console.OpenStandardInput();
        await using Stream output = Console.OpenStandardOutput();
        await StartAsync(input, output);
    }

    private async Task RunPipeAsync(string pipeName)
    {
        await using NamedPipeServerStream server = new(pipeName);
        Console.WriteLine($"Waiting On Pipe with name {pipeName}");
        await server.WaitForConnectionAsync();
        Console.WriteLine($"Connected!");
        await StartAsync(server);
    }

    private async Task RunSocketAsync(int port)
    {
        using Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Any, port));
        socket.Listen();
        Console.WriteLine($"Waiting On Port {port}");
        await using NetworkStream stream = new(await socket.AcceptAsync());
        Console.WriteLine($"Connected!");
        await StartAsync(stream);
    }

    private Task StartAsync(Stream iostream) => StartAsync(iostream, iostream);
    #endregion

    private async Task StartAsync(Stream input, Stream output)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddEnvironmentVariables("SHIMAKAZE_SDK_");

        builder
            .Services
            .AddSingleton<DataManager>()
            .AddSingleton<Services.Ini.IniService>()
            .AddHostedService<LanguageServerHostedService>()
            .AddLanguageServer(
                "Shimakaze.Sdk.LanguageServer",
                options => options
                    .WithInput(input)
                    .WithOutput(output)
                    .AddHandler<TextDocumentHandler>()
                    .AddHandler<SemanticTokensHandler>()
                    .AddHandler<FoldingRangeHandler>()
            );

        IHost host = builder.Build();

        if (ClientProcessId.HasValue)
        {
            Process proc = Process.GetProcessById(ClientProcessId.Value);
            proc.Exited += (_, _) => _ = host.StopAsync();
        }

        await host.RunAsync();
    }
}
