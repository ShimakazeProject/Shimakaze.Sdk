using System.Net;

namespace Shimakaze.Sdk.Service.Web.Servlet;


/// <summary>
/// Servlet Base
/// </summary>
public abstract class HttpServlet : GenericServlet
{
    /// <inheritdoc/>
    public override void Service(HttpListenerRequest request, HttpListenerResponse response)
    {
        (request.HttpMethod switch
        {
            "GET" => OnGet,
            "HEAD" => OnHead,
            "POST" => OnPost,
            "PUT" => OnPut,
            "DELETE" => OnDelete,
            "CONNECT" => OnConnect,
            "OPTIONS" => OnOptions,
            "TRACE" => OnTrace,
            "PATCH" => OnPatch,
            _ => (ServletDelegate)OnOther
        })(request, response);
    }
    /// <summary>
    /// On GET Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnGet(HttpListenerRequest request, HttpListenerResponse response) { }
    /// <summary>
    /// On HEAD Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnHead(HttpListenerRequest request, HttpListenerResponse response) { }
    /// <summary>
    /// On POST Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnPost(HttpListenerRequest request, HttpListenerResponse response) { }

    /// <summary>
    /// On PUT Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnPut(HttpListenerRequest request, HttpListenerResponse response) { }
    /// <summary>
    /// On DELETE Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnDelete(HttpListenerRequest request, HttpListenerResponse response) { }
    /// <summary>
    /// On CONNECT Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnConnect(HttpListenerRequest request, HttpListenerResponse response) { }
    /// <summary>
    /// On OPTIONS Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnOptions(HttpListenerRequest request, HttpListenerResponse response) { }
    /// <summary>
    /// On TRACE Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnTrace(HttpListenerRequest request, HttpListenerResponse response) { }
    /// <summary>
    /// On PATCH Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnPatch(HttpListenerRequest request, HttpListenerResponse response) { }

    /// <summary>
    /// On Other Request
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <param name="response">Web Response</param>
    protected virtual void OnOther(HttpListenerRequest request, HttpListenerResponse response) { }
}