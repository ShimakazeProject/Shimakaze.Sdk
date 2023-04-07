
using System.Globalization;

using Irony.Parsing;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Ini语法
/// </summary>
[Language("INI", "0.0.1", "Ini for Shimakaze.Sdk")]
public sealed class IniGrammar : Grammar
{
    /// <summary>
    /// Ini语法
    /// </summary>
    public IniGrammar()
    {
        GrammarComments = """
        This syntax is for Shimakaze.Sdk's INI.
        It may not useful for you.
        """;

        IdentifierTerminal sectionIndentifier = new("Section Indentifier");
        IdentifierTerminal key = new("Key");
        FreeTextLiteral valueContent = new("Value String", "\r", "\n");
        CommentTerminal semiComment = new("SemiComment", ";", "\r", "\n");
        CommentTerminal hashComment = new("HashComment", "#", "\r", "\n");

        KeyTerm eq = ToTerm("=");
        KeyTerm lb = ToTerm("[");
        KeyTerm rb = ToTerm("]");

        NonTerminal document = new("Document");
        NonTerminal sections = new("Section List");
        NonTerminal section = new("Section");
        NonTerminal sectionHeader = new("Section Header");
        NonTerminal keyValuePairs = new("KeyValuePair List");
        NonTerminal keyValuePair = new("KeyValuePair");
        NonTerminal value = new("Value");

        Root = document;
        document.Rule
            = sections + Eof
            | keyValuePairs + sections + Eof
            ;
        sections.Rule
            = MakePlusRule(sections, section)
            ;
        section.Rule
            = sectionHeader + keyValuePairs
            ;
        sectionHeader.Rule
            = lb + sectionIndentifier + rb
            ;
        keyValuePairs.Rule
            = MakePlusRule(keyValuePairs, keyValuePair)
            ;
        keyValuePair.Rule
            = key + eq + value
            ;
        value.Rule
            = valueContent
            ;
        MarkTransient(value);

        NonGrammarTerminals.Add(semiComment);
        NonGrammarTerminals.Add(hashComment);

        LanguageFlags
            = LanguageFlags.NewLineBeforeEOF
            | LanguageFlags.CreateAst
            ;
    }
}