﻿using System.Collections;
using System.ComponentModel;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf Data.
/// </summary>
public record CsfData : IList<CsfValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    public CsfData()
      : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    /// <param name="labelName">labelName.</param>
    public CsfData(string labelName)
      : this(labelName, new List<CsfValue>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    /// <param name="labelName">labelName.</param>
    /// <param name="values">values.</param>
    public CsfData(string labelName, IEnumerable<CsfValue> values)
      : this(CsfConstants.LblFlagRaw, 1, labelName.Length, labelName, values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    /// <param name="identifier">identifier.</param>
    /// <param name="stringCount">stringCount.</param>
    /// <param name="labelNameLength">labelNameLength.</param>
    /// <param name="labelName">labelName.</param>
    public CsfData(int identifier, int stringCount, int labelNameLength, string labelName)
      : this(CsfConstants.LblFlagRaw, 1, labelName.Length, labelName, new List<CsfValue>(stringCount))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    /// <param name="identifier">identifier.</param>
    /// <param name="stringCount">stringCount.</param>
    /// <param name="labelNameLength">labelNameLength.</param>
    /// <param name="labelName">labelName.</param>
    /// <param name="values">values.</param>
    public CsfData(int identifier, int stringCount, int labelNameLength, string labelName, IEnumerable<CsfValue> values)
    {
        Identifier = identifier;
        StringCount = stringCount;
        LabelNameLength = labelNameLength;
        LabelName = labelName;
        Values = values.ToList();
    }

    /// <summary>
    /// Gets or sets identifier.
    /// </summary>
    [DefaultValue(CsfConstants.LblFlagRaw)]
    public int Identifier { get; set; }

    /// <summary>
    /// Gets or sets stringCount.
    /// </summary>
    public int StringCount { get; set; }

    /// <summary>
    /// Gets or sets labelNameLength.
    /// </summary>
    public int LabelNameLength { get; set; }

    /// <summary>
    /// Gets or sets labelName.
    /// </summary>
    public string LabelName { get; set; }

    /// <summary>
    /// Gets or sets values.
    /// </summary>
    public IList<CsfValue> Values { get; set; }

    /// <inheritdoc/>
    public int Count => Values.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => Values.IsReadOnly;

    /// <inheritdoc/>
    public CsfValue this[int index] { get => Values[index]; set => Values[index] = value; }

    /// <summary>
    /// Re Count.
    /// </summary>
    public void ReCount()
    {
        StringCount = Values.Count;
    }

    /// <inheritdoc/>
    public int IndexOf(CsfValue item)
    {
        return Values.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, CsfValue item)
    {
        Values.Insert(index, item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        Values.RemoveAt(index);
    }

    /// <inheritdoc/>
    public void Add(CsfValue item)
    {
        Values.Add(item);
    }

    /// <summary>
    /// Add Value.
    /// </summary>
    /// <param name="value">value.</param>
    public void Add(string value)
    {
        Add(new CsfValue(value));
    }

    /// <summary>
    /// Add Extra Value.
    /// </summary>
    /// <param name="value">value.</param>
    /// <param name="extra">extra.</param>
    public void Add(string value, string extra)
    {
        Add(new CsfValueExtra(value, extra));
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Values.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(CsfValue item)
    {
        return Values.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(CsfValue[] array, int arrayIndex)
    {
        Values.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(CsfValue item)
    {
        return Values.Remove(item);
    }

    /// <inheritdoc/>
    public IEnumerator<CsfValue> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Values).GetEnumerator();
    }
}
