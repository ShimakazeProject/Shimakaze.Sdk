using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Models.Ini.TerrainControl;
public sealed class TerrainControlConfig : IIniSection
{
    private readonly IIniSection _iniSection;

    public TerrainControlConfig(IIniSection iniSection)
    {
        _iniSection = iniSection;
    }

    public int? TilesInSet
    {
        get => this[nameof(TilesInSet)]?.ToInt32();
        set => this[nameof(TilesInSet)] = value.ToString();
    }

    public int? LastTilesInSet
    {
        get => this[nameof(LastTilesInSet)]?.ToInt32();
        set => this[nameof(LastTilesInSet)] = value.ToString();
    }

    public string? SetName
    {
        get => this[nameof(SetName)];
        set => this[nameof(SetName)] = value;
    }
    public string? FileName
    {
        get => this[nameof(FileName)];
        set => this[nameof(FileName)] = value;
    }

    public int? MarbleMadness
    {
        get => this[nameof(MarbleMadness)]?.ToInt32();
        set => this[nameof(MarbleMadness)] = value.ToString();
    }
    
    public int? NonMarbleMadness
    {
        get => this[nameof(NonMarbleMadness)]?.ToInt32();
        set => this[nameof(NonMarbleMadness)] = value.ToString();
    }


    #region IIniSection
    public IIniLine this[int index] { get => _iniSection[index]; set => _iniSection[index] = value; }
    public string? this[string key] { get => _iniSection[key]; set => _iniSection[key] = value; }

    public IIniLine[]? BeforeSummaries { get => _iniSection.BeforeSummaries; set => _iniSection.BeforeSummaries = value; }
    public string Name { get => _iniSection.Name; set => _iniSection.Name = value; }
    public string? Summary { get => _iniSection.Summary; set => _iniSection.Summary = value; }

    public void Add(IIniLine kvp)
    {
        _iniSection.Add(kvp);
    }

    public IEnumerator<IIniLine> GetEnumerator()
    {
        return _iniSection.GetEnumerator();
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? result)
    {
        return _iniSection.TryGetValue(key, out result);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_iniSection).GetEnumerator();
    }
    #endregion
}
