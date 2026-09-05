using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Provides static methods to serialize objects to <see cref="IniSection"/> and
/// deserialize <see cref="IniSection"/> back to .NET objects.
/// </summary>
/// <remarks>
/// Value converters registered in <see cref="IniSerializerOptions.Converters"/> are
/// consulted during individual value conversion. A converter that matches the top-level
/// type handles simple types directly; complex types fall through to
/// <see cref="IniTypeInfo"/>-based dispatch.
/// </remarks>
public static class IniSerializer
{
    // ==================== Serialize ====================

    /// <summary>
    /// Serializes an object to an <see cref="IniSection"/>.
    /// Properties marked with <see cref="IniIgnoreAttribute"/> are skipped;
    /// <see cref="IniKeyAttribute"/> controls the key name;
    /// <see cref="IniInlineArrayAttribute"/> serializes collections as a joined string.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">Serialization options.</param>
    [RequiresDynamicCode("IniTypeInfo.Create uses Array.CreateInstance.")]
    [RequiresUnreferencedCode("IniTypeInfo.Create uses reflection.")]
    public static IniSection Serialize<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.Interfaces)] T>(
        T value, IniSerializerOptions? options = null)
    {
        options ??= IniSerializerOptions.Default;

        var converter = options.GetConverter<T>();
        if (converter is not null)
            return WrapSingleValue(converter.Write(value, options));

        var typeInfo = IniTypeInfo.Create<T>();
        return Serialize(value, typeInfo, options);
    }

    /// <summary>
    /// Serializes using pre-computed <see cref="IniTypeInfo{T}"/> metadata.
    /// </summary>
    public static IniSection Serialize<T>(
        T value, IniTypeInfo<T> typeInfo, IniSerializerOptions? options = null)
    {
        options ??= IniSerializerOptions.Default;

        var converter = options.GetConverter<T>();
        if (converter is not null)
            return WrapSingleValue(converter.Write(value, options));

        return SerializeDispatch(value, typeInfo, options);
    }

    /// <inheritdoc cref="Serialize{T}(T, IniSerializerOptions?)"/>
    [RequiresDynamicCode("Reflection-based dispatch.")]
    [RequiresUnreferencedCode("Reflection-based dispatch may not be trim-compatible.")]
    public static IniSection Serialize(
        object? value, Type type, IniSerializerOptions? options = null)
    {
        options ??= IniSerializerOptions.Default;

        var converter = options.GetConverter(type);
        if (converter is not null)
            return WrapSingleValue(converter.WriteObject(value, options));

        return (IniSection)typeof(IniSerializer)
            .GetMethod(nameof(Serialize), 1, BindingFlags.Public | BindingFlags.Static, null,
                [type, typeof(IniSerializerOptions)], null)!
            .MakeGenericMethod(type)
            .Invoke(null, [value, options])!;
    }

    // ==================== Deserialize ====================

    /// <summary>
    /// Deserializes an <see cref="IniSection"/> using pre-computed <see cref="IniTypeInfo{T}"/> metadata.
    /// </summary>
#if !NET9_0_OR_GREATER
    [RequiresDynamicCode("Uses Array.CreateInstance and MakeGenericType.")]
#endif
    public static T Deserialize<T>(
        IniSection section, IniTypeInfo<T> typeInfo, IniSerializerOptions? options = null)
    {
        options ??= IniSerializerOptions.Default;
        return DeserializeDispatch(section, typeInfo, options);
    }

    /// <summary>
    /// Deserializes an <see cref="IniSection"/> to the specified type.
    /// </summary>
    [RequiresDynamicCode("IniTypeInfo.Create uses Array.CreateInstance.")]
    [RequiresUnreferencedCode("IniTypeInfo.Create uses reflection.")]
    public static T? Deserialize<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.Interfaces |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        IniSection section, IniSerializerOptions? options = null)
    {
        options ??= IniSerializerOptions.Default;

        var converter = options.GetConverter<T>();
        if (converter is not null)
            return converter.Read(GetFirstValue(section), options);

        var typeInfo = IniTypeInfo.Create<T>();
        return Deserialize(section, typeInfo, options);
    }

    /// <inheritdoc cref="Deserialize{T}(IniSection, IniSerializerOptions?)"/>
    [RequiresDynamicCode("Reflection-based dispatch.")]
    [RequiresUnreferencedCode("Reflection-based dispatch may not be trim-compatible.")]
    public static object? Deserialize(
        IniSection section, Type type, IniSerializerOptions? options = null)
    {
        options ??= IniSerializerOptions.Default;

        var converter = options.GetConverter(type);
        if (converter is not null)
            return converter.ReadObject(GetFirstValue(section), type, options);

        return typeof(IniSerializer)
            .GetMethod(nameof(Deserialize), 1, BindingFlags.Public | BindingFlags.Static, null,
                [typeof(IniSection), typeof(IniSerializerOptions)], null)!
            .MakeGenericMethod(type)
            .Invoke(null, [section, options]);
    }

    // ==================== Serialize dispatch ====================

    private static IniSection SerializeDispatch<T>(
        T value, IniTypeInfo<T> typeInfo, IniSerializerOptions options) => typeInfo.Kind switch
        {
            IniTypeInfoKind.Object => SerializeObject(value, typeInfo, options),
            IniTypeInfoKind.Array => SerializeEnumerable((IEnumerable)(object)value!, options),
            IniTypeInfoKind.Collection => SerializeEnumerable((IEnumerable)(object)value!, options),
            IniTypeInfoKind.Dictionary => SerializeDictionary((IDictionary)(object)value!, options),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(IniTypeInfoKind)}: {typeInfo.Kind}"),
        };

    private static IniSection SerializeObject<T>(
        T value, IniTypeInfo<T> typeInfo, IniSerializerOptions options)
    {
        IniSection section = [];
        var properties = typeInfo.Properties!;

        foreach (var prop in properties)
        {
            object? propValue = prop.GetValue(value!);

            if (propValue is null)
            {
                section.Add(prop.Key, string.Empty);
                continue;
            }

            // [IniInlineArray]: join elements with separator
            if (prop.InlineArraySeparator is { } sep
                && propValue is IEnumerable enumerable and not string)
            {
                List<string> parts = [];
                foreach (object? item in enumerable)
                    parts.Add(ConvertToString(item, item?.GetType(), options) ?? string.Empty);
                section.Add(prop.Key, string.Join(sep, parts));
                continue;
            }

            section.Add(prop.Key,
                ConvertToString(propValue, prop.PropertyType, options) ?? string.Empty);
        }

        return section;
    }

    private static IniSection SerializeEnumerable(
        IEnumerable enumerable, IniSerializerOptions options)
    {
        IniSection section = [];
        foreach (object? item in enumerable)
        {
            section.Add(string.Empty,
                ConvertToString(item, item?.GetType(), options) ?? string.Empty);
        }
        return section;
    }

    private static IniSection SerializeDictionary(
        IDictionary dictionary, IniSerializerOptions options)
    {
        IniSection section = [];
        foreach (DictionaryEntry entry in dictionary)
        {
            string key = ConvertToString(entry.Key, entry.Key.GetType(), options) ?? string.Empty;
            string val = ConvertToString(entry.Value, entry.Value?.GetType(), options) ?? string.Empty;
            section.Add(key, val);
        }
        return section;
    }

    // ==================== Deserialize dispatch ====================

#if !NET9_0_OR_GREATER
    [RequiresDynamicCode("Uses Array.CreateInstance and MakeGenericType.")]
#endif
    private static T DeserializeDispatch<T>(
        IniSection section, IniTypeInfo<T> typeInfo, IniSerializerOptions options) => typeInfo.Kind switch
        {
            IniTypeInfoKind.Object => DeserializeObject(section, typeInfo, options),
            IniTypeInfoKind.Array => DeserializeArray(section, typeInfo, options),
            IniTypeInfoKind.Collection => DeserializeCollection(section, typeInfo, options),
            IniTypeInfoKind.Dictionary => DeserializeDictionary(section, typeInfo, options),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(IniTypeInfoKind)}: {typeInfo.Kind}"),
        };

#if !NET9_0_OR_GREATER
    [RequiresDynamicCode("Uses Array.CreateInstance and MakeGenericType.")]
#endif
    private static T DeserializeObject<T>(
        IniSection section, IniTypeInfo<T> typeInfo, IniSerializerOptions options)
    {
        var ctorParams = typeInfo.ConstructorParameters!;
        var properties = typeInfo.Properties!;

        // Build key→value lookup (case-insensitive)
        Dictionary<string, string> map = new(section.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var kv in section)
            map[kv.Key] = kv.Value;

        // Resolve constructor arguments
        object?[] args = new object?[ctorParams.Length];
        HashSet<string> consumedKeys = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < ctorParams.Length; i++)
        {
            var param = ctorParams[i];

            if (!string.IsNullOrEmpty(param.Key)
                && map.TryGetValue(param.Key, out string? strValue))
            {
                args[i] = ConvertFromString(strValue, param.ParameterType, options);
                consumedKeys.Add(param.Key);
            }
            else if (param.HasDefaultValue)
            {
                args[i] = param.DefaultValue;
            }
            else if (param.ParameterType.IsValueType)
            {
                args[i] = Activator.CreateInstance(param.ParameterType);
            }
        }

        // Create instance
        var obj = typeInfo.CreateObject!(args);

        // Set writable properties not already consumed by constructor
        foreach (var prop in properties)
        {
            if (!prop.CanWrite)
                continue;
            if (!map.TryGetValue(prop.Key, out string? strValue))
                continue;
            if (consumedKeys.Contains(prop.Key))
                continue;

            // [IniInlineArray]: split and populate collection
            if (prop.InlineArraySeparator is { } sep
                && IsEnumerableType(prop.PropertyType))
            {
                SetInlineArrayProperty(obj, prop, strValue, sep, options);
                continue;
            }

            object? converted = ConvertFromString(strValue, prop.PropertyType, options);
            prop.SetValue(obj!, converted);
        }

        return obj;
    }

    private static T DeserializeArray<T>(
        IniSection section, IniTypeInfo<T> typeInfo, IniSerializerOptions options)
    {
        var elementType = typeInfo.ValueType!;
        var array = (Array)(object)typeInfo.CreateObject!([section.Count])!;

        int index = 0;
        foreach (var item in section)
            array.SetValue(ConvertFromString(item.Value, elementType, options), index++);

        return (T)(object)array;
    }

    private static T DeserializeCollection<T>(
        IniSection section, IniTypeInfo<T> typeInfo, IniSerializerOptions options)
    {
        var elementType = typeInfo.ValueType!;
        var collection = typeInfo.CreateObject!(null);

        if (collection is IList list)
        {
            foreach (var item in section)
                list.Add(ConvertFromString(item.Value, elementType, options));
        }

        return collection;
    }

    private static T DeserializeDictionary<T>(
        IniSection section, IniTypeInfo<T> typeInfo, IniSerializerOptions options)
    {
        var keyType = typeInfo.KeyType!;
        var valueType = typeInfo.ValueType!;
        var dictionary = typeInfo.CreateObject!(null);

        if (dictionary is IDictionary dict)
        {
            foreach (var item in section)
            {
                object? key = ConvertFromString(item.Key, keyType, options);
                object? val = ConvertFromString(item.Value, valueType, options);
                dict[key!] = val;
            }
        }

        return dictionary;
    }

    // ==================== Value conversion ====================

    private static string? ConvertToString(
        object? value, Type? type, IniSerializerOptions options)
    {
        if (value is null)
            return null;

        var resolvedType = type ?? value.GetType();
        return options.GetConverter(resolvedType)?.WriteObject(value, options)
            ?? value.ToString();
    }

    private static object? ConvertFromString(
        string value, Type targetType, IniSerializerOptions options)
    {
        var converter = options.GetConverter(targetType);
        if (converter is not null)
            return converter.ReadObject(value, targetType, options);

        var underlying = Nullable.GetUnderlyingType(targetType);
        if (underlying is not null)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            targetType = underlying;
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    // ==================== Inline array helpers ====================
#if !NET9_0_OR_GREATER
    [RequiresDynamicCode("Uses Array.CreateInstance and MakeGenericType.")]
#endif
    private static void SetInlineArrayProperty<T>(
        T obj, IniPropertyInfo prop, string value, string separator,
        IniSerializerOptions options)
    {
        var propType = prop.PropertyType;
        var elementType = GetElementType(propType);

        // Inline-array elements must be simple types backed by a registered converter
        var converter = options.GetConverter(elementType)
            ?? throw new InvalidOperationException(
                $"No {nameof(IniValueConverter)} registered for inline-array element type '{elementType}'. " +
                "Inline-array properties only support simple types with registered value converters.");

        string[] parts = value.Split(separator);
#if NET9_0_OR_GREATER
        var array = Array.CreateInstanceFromArrayType(propType, parts.Length);
#else
        Array array = Array.CreateInstance(elementType, parts.Length);
#endif
        for (int i = 0; i < parts.Length; i++)
            array.SetValue(converter.ReadObject(parts[i].Trim(), elementType, options), i);

        if (propType.IsArray)
        {
            prop.SetValue(obj!, array);
            return;
        }

        if (Activator.CreateInstance(propType) is IList collection)
        {
            foreach (object? item in array)
                collection.Add(item);

            prop.SetValue(obj!, collection);
        }
    }

    private static Type GetElementType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        if (type.IsArray)
            return type.GetElementType()!;

        if (type.GetInterfaces()
            .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(i => i.GetGenericArguments()[0])
            .FirstOrDefault() is { } element)
        {
            return element;
        }

        return typeof(string);
    }

    private static bool IsEnumerableType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        if (type == typeof(string))
            return false;
        if (typeof(IDictionary).IsAssignableFrom(type))
            return false;

        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return false;
        }

        return typeof(IEnumerable).IsAssignableFrom(type);
    }

    // ==================== Utilities ====================

    private static IniSection WrapSingleValue(string? value)
    {
        IniSection section = [];
        if (value is not null)
            section.Add(string.Empty, value);
        return section;
    }

    private static string GetFirstValue(IniSection section)
    {
        foreach (var kv in section)
            return kv.Value;
        return string.Empty;
    }
}
