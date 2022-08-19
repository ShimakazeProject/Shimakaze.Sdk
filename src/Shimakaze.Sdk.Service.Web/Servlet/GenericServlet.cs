using System.Net;

namespace Shimakaze.Sdk.Service.Web.Servlet;

/// <summary>
/// Generic Servlet
/// </summary>
public abstract class GenericServlet : IServlet
{
    /// <summary>
    /// Is Disposed
    /// </summary>
    protected bool _disposedValue;

    /// <summary>
    /// Empty Constrator
    /// </summary>
    public GenericServlet() { }
    /// <summary>
    /// Invoke Dispose(bool) Method
    /// </summary>
    ~GenericServlet() => Dispose(disposing: false);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public virtual void Init() { }

    /// <inheritdoc/>
    public abstract void Service(HttpListenerRequest request, HttpListenerResponse response);

    /// <summary>
    /// Dispose
    /// </summary>
    /// <param name="disposing">disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
            _disposedValue = true;

    }
}
