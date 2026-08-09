namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Specifies the spacing style for the equals sign in key-value pairs.
/// </summary>
public enum IniKeyValueSpacing
{
    /// <summary>
    /// No spaces around equals: <c>key=value</c>
    /// </summary>
    None,

    /// <summary>
    /// Spaces around equals: <c>key = value</c>
    /// </summary>
    Spaces,
}

/// <summary>
/// Options that control the behaviour of <see cref="IniSerializer"/>.
/// Analogous to <c>System.Text.Json.JsonSerializerOptions</c>.
/// </summary>
public sealed class IniSerializerOptions
{
    private static readonly Lazy<IniBasicConverter> BasicConverter = new(() => new());

    /// <summary>
    /// Gets the default options singleton.
    /// </summary>
    public static IniSerializerOptions Default => field ??= new();

    /// <summary>
    /// Gets or sets the spacing style for the equals sign in key-value pairs.
    /// Default is <see cref="IniKeyValueSpacing.None"/>.
    /// </summary>
    public IniKeyValueSpacing KeyValueSpacing { get; set; } = IniKeyValueSpacing.None;

    /// <summary>
    /// Gets the list of user-provided <see cref="IniValueConverter"/> instances.
    /// Converters are consulted in order; the first whose <see cref="IniValueConverter.CanConvert"/>
    /// returns <see langword="true"/> is used.
    /// </summary>
    public IList<IniValueConverter> Converters { get; init; } = [BasicConverter.Value];

    /// <summary>
    /// Resolves the best <see cref="IniValueConverter"/> for <paramref name="type"/> in
    /// the registered converter list.
    /// </summary>
    /// <returns>The first matching converter, or <see langword="null"/> if none match.</returns>
    public IniValueConverter? GetConverter(Type type)
    {
        for (int i = 0; i < Converters.Count; i++)
        {
            if (Converters[i].CanConvert(type))
                return Converters[i];
        }
        return null;
    }

    /// <summary>
    /// Resolves the best <see cref="IniValueConverter{T}"/> for <typeparamref name="T"/>.
    /// </summary>
    public IniValueConverter<T>? GetConverter<T>()
        => GetConverter(typeof(T)) as IniValueConverter<T>;
}
