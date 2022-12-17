using System.Text;

namespace Shimakaze.Sdk.Text.Ini;

/// <summary>
/// An Ini Data Line.
/// </summary>
public record IniKeyValuePair
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IniKeyValuePair"/> class.
    /// </summary>
    /// <param name="key">Key name.</param>
    /// <param name="value">Value.</param>
    /// <param name="comment">Comment.</param>
    public IniKeyValuePair(string? key, string? value, string? comment)
    {
        this.Key = key;
        this.Value = value;
        this.Comment = comment;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniKeyValuePair"/> class.
    /// </summary>
    public IniKeyValuePair()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniKeyValuePair"/> class.
    /// </summary>
    /// <param name="key">Key name.</param>
    /// <param name="value">Value.</param>
    public IniKeyValuePair(string? key, string? value)
    {
        this.Key = key;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniKeyValuePair"/> class.
    /// </summary>
    /// <param name="comment">Comment.</param>
    public IniKeyValuePair(string? comment)
    {
        this.Comment = comment;
    }

    /// <summary>
    /// Gets or sets key name.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Get INI String.
    /// </summary>
    /// <param name="skipComment">Do NOT get comment at result.</param>
    /// <param name="wrapSigns">Wrap Equals and Seme by Space.</param>
    /// <returns>INI format string.</returns>
    public string ToString(bool skipComment, bool wrapSigns)
    {
        StringBuilder sb = new();
        bool isEmptyKey = string.IsNullOrEmpty(this.Key);
        bool isEmptyValue = string.IsNullOrEmpty(this.Value);
        bool isEmptyComment = string.IsNullOrEmpty(this.Comment);
        if (!isEmptyKey)
        {
            sb.Append(this.Key);
        }

        if (!isEmptyValue)
        {
            if (wrapSigns && !isEmptyKey)
            {
                sb.Append(' ');
            }

            sb.Append('=');
            if (wrapSigns)
            {
                sb.Append(' ');
            }

            sb.Append(this.Value);
        }

        if (skipComment || isEmptyComment)
        {
            return sb.ToString();
        }

        if (wrapSigns && (!isEmptyKey || !isEmptyValue))
        {
            sb.Append(' ');
        }

        sb.Append(';');
        if (wrapSigns)
        {
            sb.Append(' ');
        }

        sb.Append(this.Comment);

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override string ToString() => this.ToString(false, true);
}
