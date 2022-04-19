namespace Shimakaze.Sdk.Models.Csf;
internal static class Asserts
{
    public static int CheckMetadataFlags(int flag)
        => flag is Constants.CSF_FLAG_RAW ? flag
        : throw new CsfException("It's not CSF File Flag.");
    public static int CheckDataFlags(int flag)
        => flag is Constants.LBL_FLAG_RAW ? flag
        : throw new CsfException("It's not CSF Label Flag.");
    public static int CheckValueFlags(int flag)
        => flag is Constants.STR_FLAG_RAW or Constants.STRW_FLG_RAW ? flag
        : throw new CsfException("It's not CSF String Flag");
    public static bool TryValueFlagsIsExtra(int flag)
        => flag is Constants.STRW_FLG_RAW;
}

[Serializable]
public class CsfException : Exception
{
    public CsfException() { }
    public CsfException(string message) : base(message) { }
    public CsfException(string message, Exception inner) : base(message, inner) { }
    protected CsfException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}