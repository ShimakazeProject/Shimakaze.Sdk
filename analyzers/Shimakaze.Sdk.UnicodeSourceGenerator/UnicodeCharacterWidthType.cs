namespace Shimakaze.Sdk.UnicodeSourceGenerator;

internal enum UnicodeCharacterWidthType : byte
{
    Ambiguous,
    Fullwidth,
    Halfwidth,
    Neutral,
    Narrow,
    Wide,
}
