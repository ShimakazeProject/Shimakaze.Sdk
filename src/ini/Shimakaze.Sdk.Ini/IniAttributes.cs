namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Abstract base class for all INI-related attributes.
/// Serves as a marker so that all custom INI attributes can be distinguished
/// from other <see cref="Attribute"/> types at a glance.
/// </summary>
public abstract class IniAttribute : Attribute
{
}

/// <summary>
/// Marks a constructor as the preferred entry point for INI deserialization.
/// When applied, the serializer uses this constructor to create instances of the type.
/// If no constructor has this attribute, the serializer falls back to the parameterless
/// constructor, then to the constructor with the fewest parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
public sealed class IniConstructorAttribute : IniAttribute
{
}


/// <summary>
/// Specifies that a property or constructor parameter should be ignored during
/// INI serialization and deserialization.
/// When applied to a constructor parameter, the serializer skips matching it to any INI key
/// and uses the parameter's default value instead.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class IniIgnoreAttribute : IniAttribute
{
}

/// <summary>
/// Specifies a custom key name for a property during INI serialization and deserialization.
/// If not applied, the property name is used as the key.
/// </summary>
/// <param name="name">The key name to use in INI output.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class IniKeyAttribute(string name) : IniAttribute
{
    /// <summary>
    /// Gets the custom key name.
    /// </summary>
    public string Name { get; } = name;
}

/// <summary>
/// Specifies that a property value should be serialized as an array using the given separator.
/// For example, <c>[IniArray(", ")]</c> on an <c>int[]</c> property will serialize as <c>1, 2, 3</c>
/// and deserialize a comma-separated value back into the array.
/// </summary>
/// <param name="separator">The separator string used to split/join array elements.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class IniInlineArrayAttribute(string separator) : IniAttribute
{
    /// <summary>
    /// Gets the separator string used to split/join array elements.
    /// </summary>
    public string Separator { get; } = separator;
}
