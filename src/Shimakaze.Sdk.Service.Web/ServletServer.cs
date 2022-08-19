using System.Net;

using Shimakaze.WebServer.Locales;
using Shimakaze.Sdk.Service.Web.Servlet;

namespace Shimakaze.Sdk.Service.Web;

/// <summary>
/// Servlet Server
/// </summary>
public class ServletServer : IDisposable
{

    private const string SERVER = nameof(ServletServer);
    private readonly HttpListener _listener = new();
    private readonly Dictionary<string, ServletInfo> _servlets = new();
    private bool _disposedValue;

    /// <summary>
    /// Create Servlet Server
    /// </summary>
    /// <param name="authentication"></param>
    /// <param name="uriPrefixs"></param>
    public ServletServer(AuthenticationSchemes authentication, params string[] uriPrefixs) : this()
    {
        _listener.AuthenticationSchemes = authentication;
        uriPrefixs.Each(_listener.Prefixes.Add);
    }

    /// <summary>
    /// Create Servlet Server
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <param name="useSsl"></param>
    public ServletServer(string host, ushort port, bool useSsl = false)
        : this(AuthenticationSchemes.Anonymous, $"http{(useSsl ? "s" : string.Empty)}://{host}:{port}/") { }

    /// <summary>
    /// Initialize Servlet
    /// </summary>
    protected ServletServer()
    {
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(i => i.GetExportedTypes())
            .Where(t => t.IsAssignableTo(typeof(IServlet)))
            .Where(t => t.CustomAttributes.Any(attr => attr.AttributeType == typeof(WebServletAttribute)))
            .Select(t => (Servlet: t, Attribute: (WebServletAttribute)t.GetCustomAttributes(typeof(WebServletAttribute), false)[0]))
            .Each(i => _servlets.Add(FindServlet(i.Attribute).Path, new(i)));
    }

    /// <summary>
    /// Finalize
    /// </summary>
    ~ServletServer() => Dispose(disposing: false);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Run Servlet Server
    /// </summary>
    /// <returns></returns>
    public async Task<int> Run()
    {
        try
        {
            Logger.Info(SERVER, Locale.LogServerStart);
            _listener.Start();
            _listener.Prefixes.Each(i => Logger.Info(SERVER, Locale.LogServerListenAt, i));
            while (true)
            {
                try
                {
                    Logger.Info(SERVER, Locale.LogServerListening);
                    HttpListenerContext context = await _listener.GetContextAsync();
                    Process(context);
                }
                catch (Exception e)
                {
                    Logger.Error(SERVER, Locale.LogExceptionThrow, e);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Fatal(SERVER, Locale.LogFatalExceptionThrow, e);
            return 1;
        }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                (_listener as IDisposable)?.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Find Servlet
    /// </summary>
    /// <param name="servletAttribute"></param>
    /// <returns></returns>
    protected virtual WebServletAttribute FindServlet(WebServletAttribute servletAttribute)
    {
        Logger.Info(nameof(FindServlet), Locale.LogFindServlet, servletAttribute);
        return servletAttribute;
    }

    /// <summary>
    /// On Handler
    /// </summary>
    /// <param name="context"></param>
    protected virtual void Process(HttpListenerContext context)
    {
        Logger.Info(SERVER, Locale.LogServerHandler);
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        Logger.Info(SERVER, Locale.LogServerGetRequest, request.HttpMethod, request.Url);
        Logger.Debug(SERVER, "Content Type: {0}", request.ContentType);
        string? path = request.Url?.AbsolutePath;

        if (string.IsNullOrEmpty(path))
        {
            Logger.Warn(SERVER, Locale.Log400);
            response.StatusCode = 400;
            response.StatusDescription = Locale.Err400;
            response.Close();
            return;
        }
        if (_servlets.TryGetValue(path, out var servletInfo))
        {
            Logger.Info(SERVER, Locale.Log200);
            Thread thread = new(() =>
            {
                try
                {
                    using IServlet? servlet = Activator.CreateInstance(servletInfo.Servlet, false) as IServlet;
                    if (servlet is not null)
                    {
                        servlet.Init();
                        servlet.Service(request, response);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        Logger.Warn(SERVER, Locale.Log400);
                        Logger.Warn(SERVER, ex.ToString());
                        response.StatusCode = 400;
                        response.StatusDescription = Locale.Err400;
                        using StreamWriter sw = new(response.OutputStream);
                        sw.Write(ex.ToString());
                        response.Close();
                    }
                    catch { }
                }
            });
            thread.Name = servletInfo.Attribute.Name;
            thread.Start();
        }
        else
        {
            Logger.Warn(SERVER, Locale.Log404);
            response.StatusCode = 404;
            response.StatusDescription = Locale.Err404;
            response.Close();
        }
    }
}