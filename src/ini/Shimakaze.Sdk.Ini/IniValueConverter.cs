using System.Globalization;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Non-generic base class for INI value converters.
/// Converts a single key-value pair's <em>value</em> (a raw string) to/from a typed .NET object.
/// Register derived instances in <c>IniSerializerOptions.ValueConverters</c> to
/// customize value-level conversion.
/// </summary>
/// <remarks>
/// <para><c>IniValueConverter</c> operates at the individual value level — it is invoked
/// for each property, constructor parameter, or collection element during serialization
/// and deserialization.</para>
/// <para>Prefer the strongly-typed <see cref="IniValueConverter{T}"/> for new implementations.</para>
/// </remarks>
public abstract class IniValueConverter
{
    /// <summary>
    /// Determines whether this converter can handle the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if this converter can convert values of the given type.</returns>
    public abstract bool CanConvert(Type type);

    /// <summary>
    /// Reads a string value and converts it to an instance of <paramref name="type"/>.
    /// Called by the serializer during deserialization.
    /// </summary>
    /// <param name="value">The raw INI value string.</param>
    /// <param name="type">The target CLR type.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>The converted value, or <see langword="null"/> if the string is empty and the type is nullable.</returns>
    internal abstract object? ReadObject(string value, Type type, IniSerializerOptions options);

    /// <summary>
    /// Writes a typed value to its string representation.
    /// Called by the serializer during serialization.
    /// </summary>
    /// <param name="obj">The value to convert. May be <see langword="null"/>.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>The string representation, or <see langword="null"/>.</returns>
    internal abstract string? WriteObject(object? obj, IniSerializerOptions options);
}

/// <summary>
/// Strongly-typed base class for INI value converters.
/// Override <see cref="Read"/> and <see cref="Write"/> to implement custom value conversion for <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The value type to convert.</typeparam>
public abstract class IniValueConverter<T> : IniValueConverter
{
    /// <inheritdoc />
    public sealed override bool CanConvert(Type type) => type == typeof(T);

    /// <summary>
    /// Reads a string value and converts it to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The raw INI value string.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>The converted value, or <see langword="default"/> if the string is empty.</returns>
    public abstract T? Read(string value, IniSerializerOptions options);

    /// <summary>
    /// Writes an instance of <typeparamref name="T"/> to its string representation.
    /// </summary>
    /// <param name="obj">The value to convert. May be <see langword="null"/>.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>The string representation of the value.</returns>
    public abstract string Write(T? obj, IniSerializerOptions options);

    /// <inheritdoc />
    internal sealed override object? ReadObject(string value, Type type, IniSerializerOptions options)
        => Read(value, options);

    /// <inheritdoc />
    internal sealed override string? WriteObject(object? value, IniSerializerOptions options)
        => Write((T?)value, options);
}

/// <summary>
/// Built-in value converter that handles common .NET primitive and BCL types.
/// Supports <see cref="string"/>, integer types, floating-point types, <see cref="bool"/>,
/// <see cref="char"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>,
/// <see cref="TimeSpan"/>, <see cref="Guid"/>, enums, and any <see cref="IConvertible"/>.
/// </summary>
/// <remarks>
/// <para>Bool parsing: first character <c>'1'</c>, <c>'t'</c>/<c>'T'</c>, or <c>'y'</c>/<c>'Y'</c> is <see langword="true"/>;
/// everything else is <see langword="false"/>.</para>
/// <para>All parsing uses <see cref="CultureInfo.InvariantCulture"/> for consistent behaviour across locales.</para>
/// </remarks>
public sealed class IniBasicConverter : IniValueConverter
{
    /// <inheritdoc />
    /// <remarks>
    /// Returns <see langword="true"/> for all primitive types, enums, <see cref="Guid"/>,
    /// <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/>,
    /// <see cref="string"/>, <see cref="char"/>, and any <see cref="IConvertible"/>.
    /// </remarks>
    public override bool CanConvert(Type type)
    {
        // Nullable<T> → unwrap and check the underlying type
        Type? underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            type = underlying;

        return true switch
        {
            _ when type == typeof(string) => true,
            _ when type == typeof(int) => true,
            _ when type == typeof(long) => true,
            _ when type == typeof(short) => true,
            _ when type == typeof(byte) => true,
            _ when type == typeof(float) => true,
            _ when type == typeof(double) => true,
            _ when type == typeof(decimal) => true,
            _ when type == typeof(bool) => true,
            _ when type == typeof(char) => true,
            _ when type == typeof(DateTime) => true,
            _ when type == typeof(DateTimeOffset) => true,
            _ when type == typeof(TimeSpan) => true,
            _ when type == typeof(Guid) => true,
            _ when type == typeof(uint) => true,
            _ when type == typeof(ulong) => true,
            _ when type == typeof(ushort) => true,
            _ when type == typeof(sbyte) => true,
            _ when type.IsEnum => true,
            _ => typeof(IConvertible).IsAssignableFrom(type),
        };
    }

    /// <inheritdoc />
    /// <remarks>
    /// Converts a raw INI string to the target type. For nullable types,
    /// an empty or whitespace-only string produces <see langword="null"/>.
    /// </remarks>
    internal override object? ReadObject(string value, Type targetType, IniSerializerOptions options)
    {
        Type? underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType is not null)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            targetType = underlyingType;
        }

        return true switch
        {
            _ when targetType == typeof(string) => value,
            _ when targetType == typeof(int) => int.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(long) => long.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(short) => short.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(byte) => byte.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(float) => float.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(double) => double.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(decimal) => decimal.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(bool) => value.Length > 0 && (value[0] is '1' or 't' or 'T' or 'y' or 'Y'),
            _ when targetType == typeof(char) && value.Length == 1 => value[0],
            _ when targetType == typeof(DateTime) => DateTime.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(DateTimeOffset) => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(TimeSpan) => TimeSpan.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(Guid) => Guid.Parse(value),
            _ when targetType == typeof(uint) => uint.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(ulong) => ulong.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(ushort) => ushort.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType == typeof(sbyte) => sbyte.Parse(value, CultureInfo.InvariantCulture),
            _ when targetType.IsEnum => Enum.Parse(targetType, value, ignoreCase: false),
            _ => Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture),
        };
    }

    /// <inheritdoc />
    /// <remarks>
    /// Converts a typed value to its invariant string representation.
    /// Boolean values are written as <c>"true"</c> / <c>"false"</c>.
    /// </remarks>
    internal override string? WriteObject(object? obj, IniSerializerOptions options)
    {
        if (obj is null)
            return null;

        Type type = obj.GetType();
        Type? underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            type = underlying;

        // Use IConvertible.ToString(IFormatProvider) for consistent InvariantCulture formatting
        if (obj is IConvertible convertible)
            return convertible.ToString(CultureInfo.InvariantCulture);

        return obj.ToString();
    }
}
