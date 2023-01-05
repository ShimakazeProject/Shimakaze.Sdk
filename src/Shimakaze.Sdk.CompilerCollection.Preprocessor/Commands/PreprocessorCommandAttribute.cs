namespace Shimakaze.Sdk.CompilerCollection.Preprocessor.Commands;

/// <summary>
/// PreprocessorCommand
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PreprocessorCommandAttribute : Attribute
{
    /// <summary>
    /// PreprocessorCommand Name
    /// </summary>
    public string CommandName { get; }
    /// <summary>
    /// PreprocessorCommand
    /// </summary>
    /// <param name="command">Name</param>
    public PreprocessorCommandAttribute(string command)
    {
        CommandName = command;
    }
}
