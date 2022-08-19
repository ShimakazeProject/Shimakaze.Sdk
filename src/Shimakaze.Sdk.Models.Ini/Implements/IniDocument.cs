using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Dynamic;
using System.Collections;

namespace Shimakaze.Sdk.Models.Ini.implements;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class IniDocument : DynamicObject, IEnumerable<IIniSection>, IIniDocument
{
    private readonly List<IIniSection> _sections = new();

    public IniDocument(IEnumerable<IIniSection> sections) : this(sections.ToList())
    {
    }

    public IniDocument()
    {
    }

    internal IniDocument(List<IIniSection> sections)
    {
        _sections = sections;
    }
    public int Count => _sections.Count;
    public IIniSection Default => new IniSection(nameof(Default));
    public ICollection<IIniSection> Sections => _sections;

    public IIniSection this[string section]
    {
        get => _sections.First(i => i.Name == section);
        set
        {
            var sec = _sections.FirstOrDefault(i => i.Name == section);
            if (sec is null)
            {
                _sections.Add(value);
                return;
            }
            var index = _sections.IndexOf(sec);
            _sections[index] = value;
        }
    }

    public IniValue? this[string section, string key]
    {
        get => this[section][key];
        set
        {
            if (_sections.Any(i => i.Name == section))
                this[section][key] = value;
            else
                Add(section)[key] = value;
        }
    }
    public void Add(IIniSection section) => _sections.Add(section);

    public IniSection Add(string section)
    {
        IniSection newSection = new(section);
        Add(newSection);
        return newSection;
    }

    public IEnumerator<IIniSection> GetEnumerator() => Sections.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_sections).GetEnumerator();
    }

    public bool Remove(string section)
    {
        var i = _sections.FindIndex(i => i.Name == section);
        if (i < 0)
            return false;

        _sections.RemoveAt(i);
        return true;
    }

    public override string ToString() => $"{nameof(IniDocument)}: {Count}";
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        string name = binder.Name;
        var b = TryGetValue(name, out var obj);
        result = obj;
        return b;
    }

    public bool TryGetValue(string name, [MaybeNullWhen(false)] out IIniSection? section)
    {
        section = _sections.FirstOrDefault(i => i.Name == name);
        return section is not null;
    }
    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if (value is IniSection s1)
        {
            this[binder.Name] = s1;
            return true;
        }
        return false;
    }

    private string GetDebuggerDisplay() => ToString();
}
