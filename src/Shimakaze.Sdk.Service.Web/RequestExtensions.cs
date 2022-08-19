using System.Net;

namespace Shimakaze.Sdk.Service.Web;

/// <summary>
/// Request Extension
/// </summary>
public static class RequestExtensions
{
    /// <summary>
    /// Get Query Parameters
    /// </summary>
    /// <param name="request">Web Request</param>
    /// <returns>Parameters</returns>
    public static Dictionary<string, string> GetQueries(this HttpListenerRequest request)
    {
        return string.IsNullOrWhiteSpace(request.Url?.Query)
            ? (Dictionary<string, string>)(new())
            : request.Url!.Query[1..]
                   .Split('&')
                   .Select(x => x.Split('='))
                   .ToDictionary(x => x[0], x => x[1]);
    }
}
