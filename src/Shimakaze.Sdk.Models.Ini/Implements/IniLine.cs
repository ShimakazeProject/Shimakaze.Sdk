using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shimakaze.Sdk.Models.Ini.implements;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class IniLine : IIniLine
{
    internal readonly int EqIndex;
    internal readonly uint Line;
    internal readonly string Raw;
    internal readonly int SeIndex;
    private string? _key;

    private string? _summary;

    private string? _value;

    public IniLine(string raw)
    {
        Raw = raw;
        try
        {
            if (string.IsNullOrEmpty(raw))
                return;

            int eq = EqIndex = raw.IndexOf('=');
            int se = SeIndex = raw.IndexOf(';');

            if (se >= 0)
                _summary = raw[(se + 1)..].Trim();

            if (se < 0)
                se = raw.Length;

            if (eq >= 0)
                _value = raw[(eq + 1)..se].Trim();

            if (eq < 0)
                eq = SeIndex >= 0 ? SeIndex : raw.Length;

            _key = raw[0..eq].Trim();
        }
        finally
        {
            IsEmptyKey = string.IsNullOrEmpty(_key);
            IsEmptyValue = string.IsNullOrEmpty(_value);
            IsEmptySummary = string.IsNullOrEmpty(_summary);
        }
    }

    internal IniLine(string raw, uint line) : this(raw) => Line = line;

    [MemberNotNullWhen(false, nameof(Key))]
    public bool IsEmptyKey { get; private set; }

    [MemberNotNullWhen(false, nameof(Summary))]
    public bool IsEmptySummary { get; private set; }

    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsEmptyValue { get; private set; }

    public string? Key
    {
        get => _key;
        set
        {
            _key = value;
            IsEmptyKey = string.IsNullOrWhiteSpace(value);
        }
    }

    public string? Summary
    {
        get => _summary;
        set
        {
            _summary = value;
            IsEmptySummary = false;
        }
    }

    public string? Value
    {
        get => _value;
        set
        {
            _value = value;
            IsEmptyValue = string.IsNullOrWhiteSpace(value);
        }
    }

    public string ToString(bool ignoreSummary = false)
    {
        StringBuilder sb = new();
        if (!IsEmptyKey)
            sb.Append(Key);

        if (!IsEmptyValue)
        {
            if (!IsEmptyKey)
                sb.Append(' ');
            sb.Append("= ").Append(Value);
        }

        if (!ignoreSummary && !IsEmptySummary)
        {
            if (!IsEmptyKey || !IsEmptyValue)
                sb.Append(' ');

            sb.Append("; ").Append(Summary);
        }

        return sb.ToString();
    }

    public override string ToString() => ToString(false);

    internal IniToken GetToken(int index)
    {
        switch (Raw[index])
        {
            case ' ':
                return IniToken.Space;
            case '=':
                return IniToken.Equal;
            case ';':
                return IniToken.Semicolon;
        }
        if (EqIndex >= 0 && index < EqIndex)
            return IniToken.Key;
        if (SeIndex >= 0 && index > SeIndex)
            return IniToken.Summary;
        if (index > EqIndex && index < SeIndex)
            return IniToken.Value;

        return IniToken.Key;
    }
    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}