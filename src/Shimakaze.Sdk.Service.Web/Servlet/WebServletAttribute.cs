// See https://aka.ms/new-console-template for more information

namespace Shimakaze.Sdk.Service.Web.Servlet;

/// <summary>
/// Servlet Attribute
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class WebServletAttribute : Attribute
{
    /// <summary>
    /// Servlet Attribute
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name">Servlet Name</param>
    public WebServletAttribute(string path, string name = "<Untitled Servlet>")
    {
        Path = path;
        Name = name;
    }

    /// <summary>
    /// Servlet Name
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Request Path
    /// </summary>
    public string Path { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{{{nameof(Name)}={Name}, {nameof(Path)}={Path}}}";
    }
}
