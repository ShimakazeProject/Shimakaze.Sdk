namespace Shimakaze.Sdk.Inilyn.Command;

internal static class DefineCommand
{
    [Command("define")]
    public static void Define(ParserContext context, string identifier)
    {
        HashSet<string> defines = context.GetOrNew("Defines", () => new HashSet<string>());
        defines.Add(identifier);
    }

    [Command("undef")]
    public static void Undef(ParserContext context, string identifier)
    {
        HashSet<string> defines = context.GetOrNew("Defines", () => new HashSet<string>());
        defines.Remove(identifier);
    }
}
