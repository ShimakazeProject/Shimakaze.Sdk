using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;

namespace Shimakaze.Sdk.UnicodeSourceGenerator;

internal sealed record class CharMetadata(uint Start, uint? End, UnicodeCharacterWidthType Type);

/// <summary>
/// 
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class UnicodeUtilsSourceGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var text = context.AdditionalTextsProvider.Where(i => i.Path.EndsWith("EastAsianWidth.txt", StringComparison.Ordinal));
        context.RegisterImplementationSourceOutput(text, Generate);
    }

    private void Generate(SourceProductionContext context, AdditionalText source)
    {
        if (source.GetText() is not { } text)
            return;

        Regex regex = new(@"^([0-9a-fA-F]{4,6})(\.\.([0-9a-fA-F]{4,6}))?\s+;\s+([AFHNW]a?)", RegexOptions.Compiled);
        var map = text.Lines
            .AsParallel()
            .Select(item =>
            {
                var line = text.GetSubText(item.Span);
                var match = regex.Match(line.ToString());
                if (!match.Success)
                    return null;

                uint start = uint.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                uint? end = null;
                if (match.Groups[3].Success)
                    end = uint.Parse(match.Groups[3].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                var type = AsUnicodeCharacterWidthType(match.Groups[4].Value);

                return new CharMetadata(start, end, type);
            })
            .OfType<CharMetadata>()
            .GroupBy(i => IsFullWidth(i.Type))
            .ToDictionary(i => i.Key, i => i.ToArray());

        using StringWriter code = new();
        code.WriteLine("""
        #nullable enable

        namespace System;

        internal static class UnicodeUtils
        {
            private static bool IsFullWidthCharacter(int unicode) => unicode switch
            {
        """);
        foreach (var item in map)
        {
            var b = item.Key.ToString().ToLowerInvariant();
            if (b is "false")
                continue;

            foreach (var j in item.Value)
            {
                code.Write("            ");
                var str = j.End.HasValue
                    ? $">= {j.Start} and <= {j.End} => {b},"
                    : $"{j.Start} => {b},";
                code.WriteLine(str);
            }
        }
        code.WriteLine("""
                    _ => false,
            };

            extension(char)
            {
                public static bool IsFullWidth(int unicode)
                    => IsFullWidthCharacter(unicode);
            }

            extension(ReadOnlySpan<char> str)
            {
                public int GetUnicodeWidth()
                {
                    int result = 0;
                    for (int i = 0; i < str.Length; i++)
                    {
                        int unicode = 0;
                        if (char.IsHighSurrogate(str[i]))
                        {
                            unicode |= str[i] & 0x03FF;
                            unicode <<= 10;
                            i++;
                            unicode |= str[i] & 0x03FF;
                            unicode += 0x10000;
                        }
                        else
                        {
                            unicode = str[i];
                        }

                        if (IsFullWidthCharacter(unicode))
                            result += 2;
                        else if (!char.IsControl((char)unicode))
                            result ++;
                    }
                    return result;
                }
            }
        }
        """);

        context.AddSource("UnicodeUtils.g.cs", code.ToString());
    }

    internal static UnicodeCharacterWidthType AsUnicodeCharacterWidthType(string value) => value switch
    {
        "A" => UnicodeCharacterWidthType.Ambiguous,
        "F" => UnicodeCharacterWidthType.Fullwidth,
        "H" => UnicodeCharacterWidthType.Halfwidth,
        "N" => UnicodeCharacterWidthType.Neutral,
        "Na" => UnicodeCharacterWidthType.Narrow,
        "W" => UnicodeCharacterWidthType.Wide,
        _ => throw new FormatException(),
    };

    internal static bool IsFullWidth(UnicodeCharacterWidthType value) => value switch
    {
        UnicodeCharacterWidthType.Ambiguous
        or UnicodeCharacterWidthType.Halfwidth
        or UnicodeCharacterWidthType.Neutral
        or UnicodeCharacterWidthType.Narrow => false,
        UnicodeCharacterWidthType.Fullwidth
        or UnicodeCharacterWidthType.Wide => true,
        _ => throw new InvalidCastException(),
    };
}
