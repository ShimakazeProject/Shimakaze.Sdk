using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace Shimakaze.Sdk.Models.Ini.implements;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public sealed class IniSection : DynamicObject, IEnumerable<IIniLine>, IIniSection
{
    private readonly List<IIniLine> _lines = new();
    public IniSection(string name) => Name = name;

    public string[]? BeforeSummaries { get; set; }
    public string Name { get; set; }
    public string? Summary { get; set; }
    public IniValue? this[string key]
    {
        get => _lines.First(i => i.Key == key).Value;
        set => _lines.First(i => i.Key == key).Value = value;
    }

    public IIniLine this[int index]
    {
        get => _lines[index];
        set => _lines[index] = value;
    }
    public override IEnumerable<string> GetDynamicMemberNames() => _lines.Where(i => !i.IsEmptyKey).Select(i => i.Key!);

    public IEnumerator<IIniLine> GetEnumerator()
    {
        return ((IEnumerable<IIniLine>)_lines).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_lines).GetEnumerator();
    }

    public override string ToString()
    {
        return $"[{Name}]";
    }

    public override bool TryGetMember(GetMemberBinder binder, [NotNullWhen(true)] out object? result)
    {
        string key = binder.Name;
        var b = TryGetValue(key, out var a);
        result = a;
        return b;
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out IniValue? result)
    {
        result = _lines.FirstOrDefault(i => i.Key == key)?.Value;
        return result is not null;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if (value is IniValue v)
        {
            this[binder.Name] = v;
            return true;
        }
        return false;
    }

    private string GetDebuggerDisplay() => ToString();

    public void Add(IIniLine kvp) => _lines.Add(kvp);
}
