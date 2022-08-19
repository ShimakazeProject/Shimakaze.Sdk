using System.Net;

namespace Shimakaze.Sdk.Service.Web.Servlet;


/// <summary>
/// Servlet Delegate
/// </summary>
/// <param name="request">Web Request</param>
/// <param name="response">Web Response</param>
public delegate void ServletDelegate(HttpListenerRequest request, HttpListenerResponse response);

/// <summary>
/// Servlet Info
/// </summary>
/// <param name="Attribute">Attribute</param>
/// <param name="Servlet">Servlet</param>
internal sealed record ServletInfo(WebServletAttribute Attribute, Type Servlet)
{
    internal ServletInfo((Type Servlet, WebServletAttribute Attribute) tuple) : this(tuple.Attribute, tuple.Servlet)
    { }
}