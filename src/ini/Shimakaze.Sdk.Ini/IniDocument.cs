using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Represents an INI document, which is an ordered dictionary of named sections.
/// Supports duplicate section names (stored as multiple sections per name).
/// All sections are maintained in insertion order, including duplicate names.
/// </summary>
/// <param name="data">The initial section name-to-section pairs to populate the document.</param>
public class IniDocument(IEnumerable<KeyValuePair<string, IniSection>> data) : IDictionary<string, IniSection>
{
    private readonly List<KeyValuePair<string, IniSection>> _entries = [.. data];

    /// <inheritdoc/>
    public IniSection this[string key]
    {
        get
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (StringComparer.Ordinal.Equals(_entries[i].Key, key))
                    return _entries[i].Value;
            }

            throw new KeyNotFoundException($"The section '{key}' was not found.");
        }
        set
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (StringComparer.Ordinal.Equals(_entries[i].Key, key))
                {
                    _entries[i] = KeyValuePair.Create(key, value);
                    return;
                }
            }

            _entries.Add(KeyValuePair.Create(key, value));
        }
    }

    /// <inheritdoc/>
    public KeyCollection Keys { get => field ??= new(this); private set; }

    /// <inheritdoc/>
    public ValueCollection Values { get => field ??= new(this); private set; }

    /// <inheritdoc/>
    public int Count => _entries.Count;

    ICollection<string> IDictionary<string, IniSection>.Keys => Keys;

    ICollection<IniSection> IDictionary<string, IniSection>.Values => Values;

    bool ICollection<KeyValuePair<string, IniSection>>.IsReadOnly => false;

    /// <summary>
    /// Initializes a new empty instance of the <see cref="IniDocument"/> class.
    /// </summary>
    public IniDocument() : this([])
    {
    }

    /// <inheritdoc/>
    public void Add(string key, IniSection value) => _entries.Add(KeyValuePair.Create(key, value));

    /// <inheritdoc/>
    public void Clear() => _entries.Clear();

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (StringComparer.Ordinal.Equals(_entries[i].Key, key))
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, IniSection>> GetEnumerator() => _entries.GetEnumerator();

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        bool removed = false;
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            if (StringComparer.Ordinal.Equals(_entries[i].Key, key))
            {
                _entries.RemoveAt(i);
                removed = true;
            }
        }

        return removed;
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IniSection value)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (StringComparer.Ordinal.Equals(_entries[i].Key, key))
            {
                value = _entries[i].Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Converts the document to a regular <see cref="Dictionary{TKey, TValue}"/>,
    /// keeping only the last section for each duplicate name.
    /// </summary>
    /// <returns>A dictionary with unique section names mapped to their last-occurring sections.</returns>
    public Dictionary<string, IniSection> ToDictionary()
    {
        Dictionary<string, IniSection> map = new(_entries.Count, StringComparer.Ordinal);
        foreach (var item in _entries)
            map[item.Key] = item.Value;

        return map;
    }

    // ---- INI text parsing (factory methods) ----

    /// <summary>
    /// Parses a string containing INI-formatted text into an <see cref="IniDocument"/>.
    /// </summary>
    /// <param name="text">The INI-formatted string.</param>
    /// <returns>The parsed <see cref="IniDocument"/>.</returns>
    public static IniDocument Parse(string text)
    {
        using StringReader reader = new(text);
        return Parse(reader);
    }

    /// <summary>
    /// Reads all text from a <see cref="TextReader"/> and parses it as an <see cref="IniDocument"/>.
    /// </summary>
    /// <param name="reader">The text reader containing INI text.</param>
    /// <returns>The parsed <see cref="IniDocument"/>.</returns>
    public static IniDocument Parse(TextReader reader)
    {
        IniDocument document = [];
        IniSection currentSection = [];

        // Read the first section (or global keys before any section header)
        var (nextName, _) = IniSection.Parse(reader, currentSection);

        // If we got a section name from the first parse, use it
        string? currentSectionName = nextName;

        // Loop: parse the next section, commit the previous one
        while (currentSectionName is not null)
        {
            document.Add(currentSectionName, currentSection);
            currentSection = [];
            (currentSectionName, _) = IniSection.Parse(reader, currentSection);
        }

        // If the last section has content but no name (end-of-stream), it's global keys
        if (currentSection.Count > 0)
            document.Add(string.Empty, currentSection);

        return document;
    }

    /// <summary>
    /// Reads all text from a <see cref="Stream"/> and parses it as an <see cref="IniDocument"/>.
    /// </summary>
    /// <param name="stream">The stream containing INI text.</param>
    /// <returns>The parsed <see cref="IniDocument"/>.</returns>
    public static IniDocument Parse(Stream stream)
    {
        using StreamReader reader = new(stream, null, true, -1, true);
        return Parse(reader);
    }

    /// <summary>
    /// Asynchronously reads all text from a <see cref="Stream"/> and parses it as an <see cref="IniDocument"/>.
    /// </summary>
    /// <param name="stream">The stream containing INI text.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The parsed <see cref="IniDocument"/>.</returns>
    public static async Task<IniDocument> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using StreamReader reader = new(stream, null, true, -1, true);
        string text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return Parse(text);
    }

    // ---- INI text output ----

    /// <summary>
    /// Writes this document as INI-formatted text to a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="options">Writing options. If <see langword="null"/>, defaults are used.</param>
    public void WriteTo(TextWriter writer, IniSerializerOptions? options = null)
    {
        options ??= IniSerializerOptions.Default;

        foreach (var kvp in this)
        {
            writer.Write('[');
            writer.Write(kvp.Key);
            writer.WriteLine(']');

            kvp.Value.WriteTo(writer, options);

            writer.WriteLine();
        }
    }

    /// <summary>
    /// Writes this document as INI-formatted text to a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="options">Writing options. If <see langword="null"/>, defaults are used.</param>
    public void WriteTo(Stream stream, IniSerializerOptions? options = null)
    {
        using StreamWriter writer = new(stream, null, -1, true);
        WriteTo(writer, options);
    }

    /// <summary>
    /// Converts this document to an INI-formatted string with default options.
    /// </summary>
    /// <returns>The INI-formatted string representation.</returns>
    public override string ToString() => ToString(null);

    /// <summary>
    /// Converts this document to an INI-formatted string.
    /// </summary>
    /// <param name="options">Writing options. If <see langword="null"/>, defaults are used.</param>
    /// <returns>The INI-formatted string representation.</returns>
    public string ToString(IniSerializerOptions? options)
    {
        using StringWriter writer = new();
        WriteTo(writer, options);
        return writer.ToString();
    }

    /// <summary>
    /// Asynchronously writes this document as INI-formatted text to a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="options">Writing options. If <see langword="null"/>, defaults are used.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task WriteToAsync(Stream stream, IniSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        string text = ToString(options);
        byte[] bytes = Encoding.UTF8.GetBytes(text);
#if NETSTANDARD2_0
        await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
#else
        await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
#endif
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    // ---- Explicit interface implementations ----

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection<KeyValuePair<string, IniSection>>.Add(KeyValuePair<string, IniSection> keyValuePair) =>
        _entries.Add(keyValuePair);

    bool ICollection<KeyValuePair<string, IniSection>>.Contains(KeyValuePair<string, IniSection> keyValuePair)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (StringComparer.Ordinal.Equals(_entries[i].Key, keyValuePair.Key)
                && ReferenceEquals(_entries[i].Value, keyValuePair.Value))
            {
                return true;
            }
        }

        return false;
    }

    void ICollection<KeyValuePair<string, IniSection>>.CopyTo(KeyValuePair<string, IniSection>[] array, int arrayIndex) => _entries.CopyTo(array, arrayIndex);

    bool ICollection<KeyValuePair<string, IniSection>>.Remove(KeyValuePair<string, IniSection> keyValuePair)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (StringComparer.Ordinal.Equals(_entries[i].Key, keyValuePair.Key)
                && ReferenceEquals(_entries[i].Value, keyValuePair.Value))
            {
                _entries.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A read-only collection of all section names in the document, including duplicates,
    /// in insertion order.
    /// </summary>
    /// <param name="document">The parent <see cref="IniDocument"/> instance.</param>
    public sealed class KeyCollection(IniDocument document) : ICollection<string>, ICollection, IReadOnlyCollection<string>
    {
        /// <inheritdoc/>
        public int Count => document._entries.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)document).SyncRoot;

        bool ICollection<string>.IsReadOnly => true;

        /// <inheritdoc/>
        public bool Contains(string item) => document.ContainsKey(item);

        /// <inheritdoc/>
        public void CopyTo(string[] array, int arrayIndex)
        {
            var enumerator = GetEnumerator();
            for (int i = arrayIndex; i < array.Length; i++)
            {
                enumerator.MoveNext();
                array[i] = enumerator.Current;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<string> GetEnumerator()
        {
            foreach (var entry in document._entries)
                yield return entry.Key;
        }

        void ICollection<string>.Add(string item) => throw new NotSupportedException();

        void ICollection<string>.Clear() => throw new NotSupportedException();

        void ICollection.CopyTo(Array array, int index)
        {
            var enumerator = GetEnumerator();
            for (int i = index; i < array.Length; i++)
            {
                enumerator.MoveNext();
                array.SetValue(enumerator.Current, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<string>.Remove(string item) => throw new NotSupportedException();
    }

    /// <summary>
    /// A read-only collection of all sections in the document, in insertion order.
    /// </summary>
    /// <param name="document">The parent <see cref="IniDocument"/> instance.</param>
    public sealed class ValueCollection(IniDocument document) : ICollection<IniSection>, ICollection, IReadOnlyCollection<IniSection>
    {
        /// <inheritdoc/>
        public int Count => document._entries.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)document).SyncRoot;

        bool ICollection<IniSection>.IsReadOnly => true;

        /// <inheritdoc/>
        public bool Contains(IniSection item)
        {
            foreach (var entry in document._entries)
            {
                if (ReferenceEquals(entry.Value, item))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void CopyTo(IniSection[] array, int arrayIndex)
        {
            var enumerator = GetEnumerator();
            for (int i = arrayIndex; i < array.Length; i++)
            {
                enumerator.MoveNext();
                array[i] = enumerator.Current;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<IniSection> GetEnumerator()
        {
            foreach (var entry in document._entries)
                yield return entry.Value;
        }

        void ICollection<IniSection>.Add(IniSection item) => throw new NotSupportedException();

        void ICollection<IniSection>.Clear() => throw new NotSupportedException();

        void ICollection.CopyTo(Array array, int index)
        {
            var enumerator = GetEnumerator();
            for (int i = index; i < array.Length; i++)
            {
                enumerator.MoveNext();
                array.SetValue(enumerator.Current, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<IniSection>.Remove(IniSection item) => throw new NotSupportedException();
    }
}
