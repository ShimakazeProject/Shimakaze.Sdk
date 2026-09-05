using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Describes the deserialization strategy encoded by an <see cref="IniTypeInfo"/>.
/// </summary>
public enum IniTypeInfoKind
{
    /// <summary>
    /// Plain object deserialized via constructor parameters and property assignment.
    /// </summary>
    Object,

    /// <summary>
    /// Rank-1 array. Uses <see cref="IniTypeInfo.ValueType"/> as the element type
    /// and <see cref="IniTypeInfo.CreateObject"/> to allocate the array via <c>Array.CreateInstance</c>.
    /// </summary>
    Array,

    /// <summary>
    /// Collection implementing <see cref="ICollection{T}"/>.
    /// Uses <see cref="IniTypeInfo.ValueType"/> as the element type.
    /// </summary>
    Collection,

    /// <summary>
    /// Dictionary implementing <see cref="IDictionary{TKey, TValue}"/>.
    /// Uses <see cref="IniTypeInfo.KeyType"/> and <see cref="IniTypeInfo.ValueType"/>.
    /// </summary>
    Dictionary,
}

/// <summary>
/// Non-generic base for pre-computed INI type metadata.
/// Describes the constructor, properties, and collection/dictionary element types
/// needed to deserialize an <see cref="IniSection"/> into a .NET object.
/// </summary>
/// <remarks>
/// <para>Obtain instances via <see cref="Create(Type)"/> (non-generic) or
/// the generic <c>IniTypeInfo&lt;T&gt;.Create()</c> factory.</para>
/// <para>For source-generation scenarios, derived types populate all properties directly
/// without any reflection — no <c>RequiresDynamicCode</c> or <c>RequiresUnreferencedCode</c> needed.</para>
/// </remarks>
public abstract class IniTypeInfo
{
    /// <summary>
    /// The CLR type this metadata describes.
    /// </summary>
    public Type Type { get; init; }

    /// <summary>
    /// The deserialization strategy.
    /// </summary>
    public IniTypeInfoKind Kind { get; init; }

    /// <summary>
    /// Parameters of the selected constructor, in order.
    /// Each entry carries a pre-resolved <see cref="IniParameterInfo.Key"/> that accounts
    /// for <see cref="IniKeyAttribute"/> on the matching property.
    /// </summary>
    public IniParameterInfo[]? ConstructorParameters { get; init; }

    /// <summary>
    /// Public properties of the type (excluding those marked <see cref="IniIgnoreAttribute"/>).
    /// Each entry carries pre-resolved <see cref="IniPropertyInfo.Key"/> and
    /// <see cref="IniPropertyInfo.InlineArraySeparator"/>.
    /// </summary>
    public IniPropertyInfo[]? Properties { get; init; }

    /// <summary>
    /// The element type.
    /// <list type="bullet">
    /// <item><see cref="IniTypeInfoKind.Array"/> — the array element type.</item>
    /// <item><see cref="IniTypeInfoKind.Collection"/> — the <c>T</c> in <see cref="ICollection{T}"/>.</item>
    /// <item><see cref="IniTypeInfoKind.Dictionary"/> — the <c>TValue</c> in <see cref="IDictionary{TKey, TValue}"/>.</item>
    /// <item><see cref="IniTypeInfoKind.Object"/> — <see langword="null"/>.</item>
    /// </list>
    /// </summary>
    public Type? ValueType { get; init; }

    /// <summary>
    /// The dictionary key type.
    /// Only set when <see cref="Kind"/> is <see cref="IniTypeInfoKind.Dictionary"/>;
    /// <see langword="null"/> for all other kinds.
    /// </summary>
    public Type? KeyType { get; init; }

    /// <summary>
    /// Factory delegate that creates an instance of <see cref="Type"/>.
    /// Receives an array of constructor argument values (in the order of
    /// <see cref="ConstructorParameters"/>), or <see langword="null"/> for a parameterless constructor.
    /// Returns the newly created instance.
    /// </summary>
    /// <remarks>
    /// For arrays, the first (and only) argument is the <c>int</c> length.
    /// For objects/collections/dictionaries, the arguments correspond to
    /// <see cref="ConstructorParameters"/>.
    /// </remarks>
    public Func<object?[]?, object?>? CreateObject { get; init; }

    /// <summary>
    /// Initializes a new instance for the given <paramref name="type"/>.
    /// Derived types or source generators must call this via <c>base(type)</c>.
    /// </summary>
    /// <param name="type">The CLR type this metadata instance will describe.</param>
    internal IniTypeInfo(Type type) => Type = type;

    /// <summary>
    /// Combined <see cref="DynamicallyAccessedMemberTypes"/> flags required by <see cref="Create"/>.
    /// </summary>
    private const DynamicallyAccessedMemberTypes CreateAccessedMember =
        DynamicallyAccessedMemberTypes.Interfaces |
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicProperties;

    /// <summary>
    /// Creates an <see cref="IniTypeInfo"/> for the specified <paramref name="type"/>
    /// by resolving the runtime reflection metadata described by <see cref="CreateAccessedMember"/>.
    /// </summary>
    /// <param name="type">The CLR type to analyze.</param>
    /// <returns>A fully populated <see cref="IniTypeInfo"/> for the given type.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="type"/> is abstract or cannot be mapped to a supported kind.
    /// </exception>
    /// <exception cref="TypeAccessException">
    /// Thrown when no suitable constructor can be resolved.
    /// </exception>
    [RequiresDynamicCode("Array element-type allocation uses Array.CreateInstance which may not be available in AOT.")]
    [RequiresUnreferencedCode("Type analysis uses MakeGenericMethod and reflection-based constructor/property inspection.")]
    public static IniTypeInfo Create([DynamicallyAccessedMembers(CreateAccessedMember)] Type type) => (IniTypeInfo)typeof(IniTypeInfo)
            .GetMethod(nameof(Create), [])!
            .MakeGenericMethod(type)
            .Invoke(null, null)!;

    /// <summary>
    /// Creates an <see cref="IniTypeInfo{T}"/> for <typeparamref name="T"/>
    /// by inspecting its constructors, properties, and implemented interfaces.
    /// </summary>
    /// <typeparam name="T">The type to analyze.</typeparam>
    /// <returns>A fully populated <see cref="IniTypeInfo{T}"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <typeparamref name="T"/> is abstract or cannot be mapped to a supported kind.
    /// </exception>
    /// <exception cref="TypeAccessException">
    /// Thrown when no suitable constructor can be resolved.
    /// </exception>
#if !NET9_0_OR_GREATER
    [RequiresDynamicCode("The code for an array of the specified type might not be available.")]
#endif
    [RequiresUnreferencedCode("Type analysis uses reflection to inspect constructors, properties, and interfaces.")]
    public static IniTypeInfo<T> Create<[DynamicallyAccessedMembers(CreateAccessedMember)] T>()
    {
        var type = typeof(T);
        if (type.IsAbstract)
            throw new NotSupportedException();

        // ── Constructor resolution ──
        // Priority: [IniConstructor] > parameterless > shortest parameter list
        var constructors = type.GetConstructors();
        var constructor = constructors
            .FirstOrDefault(static i => i.GetCustomAttribute<IniConstructorAttribute>() is not null);
        constructor ??= type.GetConstructor([]);
        constructor ??= constructors.MinBy(static i => i.GetParameters().Length);
        if (constructor is null)
            throw new TypeAccessException($"找不到{type}的构造器");

        // ── Object mode ──
        if (!type.IsAssignableTo(typeof(IEnumerable)))
        {
            // Resolve property metadata: filter [IniIgnore], resolve [IniKey] and [IniInlineArray]
            IniPropertyInfo[] properties = [.. type.GetProperties()
                .Where(static p => p.GetCustomAttribute<IniIgnoreAttribute>() is null)
                .Select(static p =>
                {
                    var keyAttr = p.GetCustomAttribute<IniKeyAttribute>();
                    var arrayAttr = p.GetCustomAttribute<IniInlineArrayAttribute>();
                    return new IniPropertyInfo
                    {
                        Name = p.Name,
                        Key = keyAttr?.Name ?? p.Name,
                        PropertyType = p.PropertyType,
                        CanWrite = p.CanWrite,
                        InlineArraySeparator = arrayAttr?.Separator,
                        Getter = p.GetValue,
                        Setter = p.SetValue,
                    };
                })];

            // Build a lookup for parameter→property key resolution
            Dictionary<string, string> propKeys = new(
                properties.Length, StringComparer.OrdinalIgnoreCase);
            foreach (var prop in properties)
                propKeys[prop.Name] = prop.Key;

            // Resolve constructor parameter metadata
            IniParameterInfo[] ctorParams = [.. constructor.GetParameters()
                .Select(p =>
                {
                    var ignoreAttr = p.GetCustomAttribute<IniIgnoreAttribute>();
                    var keyAttr = p.GetCustomAttribute<IniKeyAttribute>();
                    var arrayAttr = p.GetCustomAttribute<IniInlineArrayAttribute>();

                    string? key = ignoreAttr is not null
                        ? null
                        : keyAttr?.Name
                            ?? (propKeys.TryGetValue(p.Name!, out string? resolved) ? resolved : null);

                    return new IniParameterInfo
                    {
                        Name = p.Name,
                        Key = key,
                        ParameterType = p.ParameterType,
                        InlineArraySeparator = arrayAttr?.Separator,
                        HasDefaultValue = p.HasDefaultValue,
                        DefaultValue = p.HasDefaultValue ? p.DefaultValue : null,
                    };
                })];

            return new()
            {
                Kind = IniTypeInfoKind.Object,
                ConstructorParameters = ctorParams,
                Properties = properties,
                CreateObject = args => (T)constructor.Invoke(args),
            };
        }

        // ── Array mode (rank-1 only) ──
        if (type is { IsArray: true, HasElementType: true })
        {
            if (type.GetArrayRank() is not 1)
                throw new NotSupportedException();

#if NET9_0_OR_GREATER
            return new()
            {
                Kind = IniTypeInfoKind.Array,
                ValueType = type.GetElementType(),
                CreateObject = args => (T)(object)Array.CreateInstanceFromArrayType(type, (int)args![0]!),
            };
#else
            return new()
            {
                Kind = IniTypeInfoKind.Array,
                ValueType = type.GetElementType(),
                CreateObject = args => (T)(object)Array.CreateInstance(type.GetElementType()!, (int)args![0]!),
            };
#endif
        }

        // ── Collection / Dictionary dispatch ──
        var interfaces = type.GetInterfaces();
        Type[] genericInterfaces = [.. interfaces.Where(static i => i.IsGenericType)];

        if (genericInterfaces.FirstOrDefault(static i => i.GetGenericTypeDefinition() == typeof(IDictionary<,>)) is { } rwDictionary)
        {
            var types = rwDictionary.GetGenericArguments();
            IniParameterInfo[] ctorParams = [.. constructor.GetParameters()
                .Select(static p => new IniParameterInfo
                {
                    Name = p.Name,
                    Key = null,
                    ParameterType = p.ParameterType,
                    HasDefaultValue = p.HasDefaultValue,
                    DefaultValue = p.HasDefaultValue ? p.DefaultValue : null,
                })];

            return new()
            {
                Kind = IniTypeInfoKind.Dictionary,
                KeyType = types[0],
                ValueType = types[1],
                ConstructorParameters = ctorParams,
                CreateObject = args => (T)constructor.Invoke(args),
            };
        }

        if (genericInterfaces.FirstOrDefault(static i => i.GetGenericTypeDefinition() == typeof(ICollection<>)) is { } rwCollection)
        {
            var types = rwCollection.GetGenericArguments();
            IniParameterInfo[] ctorParams = [.. constructor.GetParameters()
                .Select(static p => new IniParameterInfo
                {
                    Name = p.Name,
                    Key = null,
                    ParameterType = p.ParameterType,
                    HasDefaultValue = p.HasDefaultValue,
                    DefaultValue = p.HasDefaultValue ? p.DefaultValue : null,
                })];

            return new()
            {
                Kind = IniTypeInfoKind.Collection,
                ValueType = types[0],
                ConstructorParameters = ctorParams,
                CreateObject = args => (T)constructor.Invoke(args),
            };
        }

        throw new NotSupportedException();
    }
}

/// <summary>
/// Strongly-typed wrapper for <see cref="IniTypeInfo"/> that provides a typed
/// <see cref="CreateObject"/> delegate returning <typeparamref name="T"/> directly.
/// </summary>
/// <typeparam name="T">The target type.</typeparam>
/// <remarks>
/// <para>Uses a primary constructor to forward <c>typeof(T)</c> to the base class.</para>
/// <para>The typed <see cref="CreateObject"/> property synchronizes with the untyped
/// base.<see cref="IniTypeInfo.CreateObject"/> via a custom <see langword="init"/> accessor.</para>
/// </remarks>
public sealed class IniTypeInfo<T>() : IniTypeInfo(typeof(T))
{
    /// <summary>
    /// Typed factory delegate that creates an instance of <typeparamref name="T"/>.
    /// Setting this property automatically updates the untyped
    /// <see cref="IniTypeInfo.CreateObject"/> on the base class.
    /// </summary>
    public new Func<object?[]?, T>? CreateObject
    {
        get;
        init
        {
            field = value;
            base.CreateObject = args => value is not null ? value(args) : null;
        }
    }
}
