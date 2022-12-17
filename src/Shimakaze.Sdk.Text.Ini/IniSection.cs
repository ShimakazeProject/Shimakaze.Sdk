using System.Collections;

namespace Shimakaze.Sdk.Text.Ini;

/// <summary>
/// An Ini Section.
/// </summary>
public class IniSection : IList<IniKeyValuePair>
{
    private readonly List<IniKeyValuePair> list = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IniSection"/> class.
    /// </summary>
    /// <param name="name">Section Name.</param>
    public IniSection(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniSection"/> class.
    /// </summary>
    /// <param name="name">Section Name.</param>
    /// <param name="comment">Comment.</param>
    public IniSection(string name, string? comment)
    {
        this.Name = name;
        this.Comment = comment;
    }

    /// <summary>
    /// Gets or sets section Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <inheritdoc/>
    public int Count => this.list.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => ((ICollection<IniKeyValuePair>)this.list).IsReadOnly;

    /// <summary>
    /// Gets or sets before Comments in INI File.
    /// </summary>
    public List<IniKeyValuePair>? BeforeComments { get; set; }

    /// <summary>
    /// Get or Set Value by Key.
    /// </summary>
    /// <param name="key">Key name.</param>
    /// <returns>value.</returns>
    public string? this[string key]
    {
        get => this.list.First(i => i.Key == key).Value;
        set
        {
            var tmp = this.list.FirstOrDefault(i => i.Key == key);
            if (tmp is not null)
            {
                tmp.Value = value;
            }
            else
            {
                this.Add(key, value);
            }
        }
    }

    /// <inheritdoc/>
    public IniKeyValuePair this[int index]
    {
        get => this.list[index];
        set => this.list[index] = value;
    }

    /// <inheritdoc/>
    public IEnumerator<IniKeyValuePair> GetEnumerator() => this.list.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.list).GetEnumerator();

    /// <inheritdoc/>
    public void Add(IniKeyValuePair item) => this.list.Add(item);

    /// <summary>
    /// Add a data line in section.
    /// </summary>
    /// <param name="key">Key name.</param>
    /// <param name="value">Value.</param>
    public void Add(string? key, string? value) => this.Add(new IniKeyValuePair(key, value));

    /// <summary>
    /// Add a comment line in section.
    /// </summary>
    /// <param name="comment">Comment.</param>
    public void Add(string? comment) => this.Add(new IniKeyValuePair(comment));

    /// <summary>
    /// Add a data line in section.
    /// </summary>
    /// <param name="key">Key name.</param>
    /// <param name="value">Value.</param>
    /// <param name="comment">Comment.</param>
    public void Add(string? key, string? value, string? comment) => this.Add(new IniKeyValuePair(key, value, comment));

    /// <inheritdoc/>
    public void Clear() => this.list.Clear();

    /// <inheritdoc/>
    public bool Contains(IniKeyValuePair item) => this.list.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(IniKeyValuePair[] array, int arrayIndex) => this.list.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public bool Remove(IniKeyValuePair item) => this.list.Remove(item);

    /// <inheritdoc/>
    public int IndexOf(IniKeyValuePair item) => this.list.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, IniKeyValuePair item) => this.list.Insert(index, item);

    /// <inheritdoc/>
    public void RemoveAt(int index) => this.list.RemoveAt(index);
}
