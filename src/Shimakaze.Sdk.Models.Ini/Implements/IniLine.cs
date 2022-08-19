using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shimakaze.Sdk.Models.Ini.implements;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class IniLine : IIniLine
{
    private string? _key;
    private string? _summary;
    private IniValue? _value;
    public IniLine(string raw)
    {
        var iSemicolon = raw.IndexOf(';');
        var iEqual = raw.IndexOf('=');
        if (iSemicolon == -1)
        {
            IsNoSummary = true;
        }
        if (!IsNoSummary && iEqual > iSemicolon)
        {
            IsEmptyKey = true;
        }

        Key = raw[..iEqual].Trim();
        Value = raw[(iEqual + 1)..(IsNoSummary ? iSemicolon : raw.Length - iEqual - 1)].Trim();
        if (!IsNoSummary)
        {
            Summary = raw[(iSemicolon + 1)..].Trim();
        }
    }


    [MemberNotNullWhen(false, nameof(Key))]
    public bool IsEmptyKey { get; private set; }
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsEmptyValue { get; private set; }
    [MemberNotNullWhen(false, nameof(Summary))]
    public bool IsNoSummary { get; private set; }
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
            IsNoSummary = false;
        }
    }
    public IniValue? Value
    {
        get => _value;
        set
        {
            _value = value;
            IsEmptyValue = string.IsNullOrWhiteSpace(value?.Raw);
        }
    }
    public string ToString(bool ignoreSummary = false)
    {
        StringBuilder sb = new();
        if (!IsEmptyKey)
        {
            sb
                .Append(Key)
                .Append('=');

            if (!IsEmptyValue)
            {
                sb.Append(Value);
            }
        }
        if (!ignoreSummary && !IsNoSummary)
        {
            if (!IsEmptyKey)
            {
                sb.Append(' ');
            }

            sb
                .Append(';')
                .Append(' ')
                .Append(Summary);
        }

        return sb.ToString();
    }
    public override string ToString() => ToString(false);

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}