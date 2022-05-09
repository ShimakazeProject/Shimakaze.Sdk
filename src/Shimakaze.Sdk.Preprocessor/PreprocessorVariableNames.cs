namespace Shimakaze.Sdk.Preprocessor;

/// <summary>
/// 预处理器变量名列表<br/>
/// 变量名_类型_类型的泛型列表
/// </summary>
public static class PreprocessorVariableNames
{
#pragma warning disable CS1591
    public const string WorkingDirectory_Stack_String = "WorkingDirectory";
    public const string DefineStack_Stack_String = "DefineStack";
    public const string Defines_HashSet_String = "Defines";
    public const string WriteOutput_Boolean = "WriteOutput";
    public const string OutputStream_TextWriter = "OutputStream";
    public const string Commands_Dictionary_String_IPreprocessorCommand = "Commands";
    public const string CurrentFile_Stack_String = "CurrentFile";
#pragma warning restore CS1591
}
