using System.Collections;

namespace Shimakaze.Sdk.Text.Ini;

/// <summary>
/// An INI Document.
/// </summary>
public class IniDocument : IList<IniSection>
{
    private readonly List<IniSection> sections = new();

    /// <inheritdoc/>
    public int Count => this.sections.Count;

    /// <summary>
    /// Gets key value pairs when they are not contains at sections.
    /// </summary>
    public IniSection Default { get; } = new("; Default");

    /// <inheritdoc/>
    public bool IsReadOnly => ((ICollection<IniSection>)this.sections).IsReadOnly;

    /// <summary>
    /// Get or Set Section in INI Document.
    /// </summary>
    /// <param name="section">Section Name.</param>
    /// <returns><code cref="IniSection"/></returns>
    public IniSection this[string section]
    {
        get => this.sections.First(i => i.Name == section);
        set
        {
            IniSection? sec = this.sections.FirstOrDefault(i => i.Name == section);
            if (sec is null)
            {
                this.sections.Add(value);
                return;
            }

            int index = this.sections.IndexOf(sec);
            this.sections[index] = value;
        }
    }

    /// <summary>
    /// Get or Set Value from INI Document in Sections.
    /// </summary>
    /// <param name="section">Section Name.</param>
    /// <param name="key">Key Name.</param>
    /// <returns><code cref="IniSection"/></returns>
    public string? this[string section, string key]
    {
        get => this[section][key];
        set
        {
            if (this.sections.Any(i => i.Name == section))
            {
                this[section][key] = value;
            }
            else
            {
                this.Add(section)[key] = value;
            }
        }
    }

    /// <inheritdoc/>
    public IniSection this[int index]
    {
        get => this.sections[index];
        set => this.sections[index] = value;
    }

    /// <summary>
    /// Create a Section by name. And return this instance.
    /// </summary>
    /// <param name="section">Section name.</param>
    /// <returns>Section instance.</returns>
    public IniSection Add(string section)
    {
        IniSection newSection = new(section);
        this.Add(newSection);
        return newSection;
    }

    /// <inheritdoc/>
    public void Add(IniSection item) => this.sections.Add(item);

    /// <inheritdoc/>
    public void Clear() => this.sections.Clear();

    /// <inheritdoc/>
    public bool Contains(IniSection item) => this.sections.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(IniSection[] array, int arrayIndex) => this.sections.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<IniSection> GetEnumerator() => this.sections.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.sections).GetEnumerator();

    /// <inheritdoc/>
    public int IndexOf(IniSection item) => this.sections.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, IniSection item) => this.sections.Insert(index, item);

    /// <inheritdoc/>
    public bool Remove(IniSection item) => this.sections.Remove(item);

    /// <inheritdoc/>
    public void RemoveAt(int index) => this.sections.RemoveAt(index);
}
