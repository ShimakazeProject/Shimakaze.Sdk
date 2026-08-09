using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Represents a section in an INI file, which is an ordered dictionary of key-value pairs
/// that supports duplicate keys (stored as multiple values per key).
/// All entries are maintained in insertion order, including duplicate keys.
/// </summary>
/// <param name="data">The initial key-value pairs to populate the section.</param>
public class IniSection(IEnumerable<KeyValuePair<string, string>> data) : IDictionary<string, string>
{
    private readonly List<KeyValuePair<string, string>> _entries = [.. data];

    /// <inheritdoc/>
    public string this[string key]
    {
        get
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (StringComparer.Ordinal.Equals(_entries[i].Key, key))
                    return _entries[i].Value;
            }

            throw new KeyNotFoundException($"The key '{key}' was not found.");
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

    ICollection<string> IDictionary<string, string>.Keys => Keys;

    ICollection<string> IDictionary<string, string>.Values => Values;

    bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

    /// <summary>
    /// Initializes a new empty instance of the <see cref="IniSection"/> class.
    /// </summary>
    public IniSection() : this([])
    {
    }

    /// <inheritdoc/>
    public void Add(string key, string value)
    {
        _entries.Add(KeyValuePair.Create(key, value));
    }

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
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _entries.GetEnumerator();

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
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
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
    /// Converts the section to a regular <see cref="Dictionary{TKey, TValue}"/>,
    /// keeping only the last value for each duplicate key.
    /// </summary>
    /// <returns>A dictionary with unique keys mapped to their last-occurring values.</returns>
    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> map = new(_entries.Count, StringComparer.Ordinal);
        foreach (var item in _entries)
            map[item.Key] = item.Value;

        return map;
    }

    // ---- INI text parsing (static factory) ----

    /// <summary>
    /// Parses lines from a <see cref="TextReader"/> into key-value pairs and populates the given section.
    /// Each line should be a key=value pair (with or without spaces around =).
    /// Lines starting with ';' or '#' are treated as comments and skipped.
    /// Lines starting with '[' are treated as section headers and stop parsing.
    /// </summary>
    /// <param name="reader">The text reader to read lines from.</param>
    /// <param name="section">The section to populate with parsed key-value pairs.</param>
    /// <returns>
    /// A tuple containing:
    /// <c>nextSectionName</c> — the name of the next section if a section header was encountered,
    /// or <see langword="null"/> if the reader reached end-of-stream without encountering a section header;
    /// <c>stopped</c> — <see langword="true"/> if a section header was encountered (caller should
    /// stop and handle the next section), <see langword="false"/> if end-of-stream was reached.
    /// </returns>
    public static (string? NextSectionName, bool Stopped) Parse(TextReader reader, IniSection section)
    {
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            line = line.Trim();

            // Skip empty lines and comments
            if (line.Length == 0 || line[0] == ';' || line[0] == '#')
                continue;

            // Section header: [sectionname] — stop and return it
            if (line[0] == '[')
            {
                int endIndex = line.IndexOf(']');
                if (endIndex > 1)
                    return (line[1..endIndex].Trim(), true);

                // Malformed section header, skip
                continue;
            }

            // Key-value pair: key=value (with or without spaces around =)
            int equalsIndex = line.IndexOf('=');
            if (equalsIndex > 0)
            {
                string key = line[..equalsIndex].TrimEnd();
                string value = line[(equalsIndex + 1)..].TrimStart();
                section.Add(key, value);
            }
        }

        return (null, false);
    }

    // ---- INI text output ----

    /// <summary>
    /// Writes this section's key-value pairs to a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="options">Writing options. If <see langword="null"/>, defaults are used.</param>
    public void WriteTo(TextWriter writer, IniSerializerOptions? options = null)
    {
        options ??= IniSerializerOptions.Default;
        string equals = options.KeyValueSpacing == IniKeyValueSpacing.Spaces ? " = " : "=";
        foreach (var item in this)
        {
            writer.Write(item.Key);
            writer.Write(equals);
            writer.WriteLine(item.Value);
        }
    }

    /// <summary>
    /// Converts this section to its INI text representation (key-value lines only).
    /// </summary>
    /// <param name="options">Writing options. If <see langword="null"/>, defaults are used.</param>
    /// <returns>The INI-formatted key-value lines.</returns>
    public string ToString(IniSerializerOptions? options = null)
    {
        using StringWriter writer = new();
        WriteTo(writer, options);
        return writer.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> keyValuePair) =>
        _entries.Add(keyValuePair);

    bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> keyValuePair)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (StringComparer.Ordinal.Equals(_entries[i].Key, keyValuePair.Key)
                && string.Equals(_entries[i].Value, keyValuePair.Value, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        _entries.CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> keyValuePair)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (StringComparer.Ordinal.Equals(_entries[i].Key, keyValuePair.Key)
                && string.Equals(_entries[i].Value, keyValuePair.Value, StringComparison.Ordinal))
            {
                _entries.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A read-only collection of all keys in the section, including duplicates,
    /// in insertion order.
    /// </summary>
    /// <param name="section">The parent <see cref="IniSection"/> instance.</param>
    public sealed class KeyCollection(IniSection section) : ICollection<string>, ICollection, IReadOnlyCollection<string>
    {
        /// <inheritdoc/>
        public int Count => section._entries.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)section).SyncRoot;

        bool ICollection<string>.IsReadOnly => true;

        /// <inheritdoc/>
        public bool Contains(string item) => section.ContainsKey(item);

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
            foreach (var entry in section._entries)
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
    /// A read-only collection of all values in the section, in insertion order.
    /// </summary>
    /// <param name="section">The parent <see cref="IniSection"/> instance.</param>
    public sealed class ValueCollection(IniSection section) : ICollection<string>, ICollection, IReadOnlyCollection<string>
    {
        /// <inheritdoc/>
        public int Count => section._entries.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)section).SyncRoot;

        bool ICollection<string>.IsReadOnly => true;

        /// <inheritdoc/>
        public bool Contains(string item)
        {
            foreach (var entry in section._entries)
            {
                if (string.Equals(entry.Value, item, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

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
            foreach (var entry in section._entries)
                yield return entry.Value;
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
}
