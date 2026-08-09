using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Pre-computed metadata for a single property on an INI-serializable type.
/// </summary>
/// <remarks>
/// All properties are <see langword="init"/>-only. Construct instances via
/// property-initializer syntax — no constructor is necessary.
/// <para>
/// For reflection-based creation, assign <see cref="Getter"/> and <see cref="Setter"/>
/// from the corresponding <see cref="PropertyInfo"/> delegates.
/// Source generators may provide their own delegates or call
/// <see cref="GetValue"/> / <see cref="SetValue"/> directly if delegates are set.
/// </para>
/// </remarks>
public sealed class IniPropertyInfo
{
    /// <summary>
    /// The CLR name of the property.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The INI key name. Uses <see cref="IniKeyAttribute.Name"/> when present,
    /// otherwise falls back to <see cref="Name"/>.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// The declared type of the property.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public required Type PropertyType { get; init; }

    /// <summary>
    /// <see langword="true"/> when the property has a public setter.
    /// </summary>
    public required bool CanWrite { get; init; }

    /// <summary>
    /// The separator string from <see cref="IniInlineArrayAttribute"/>.
    /// <see langword="null"/> when the property is not marked with the attribute.
    /// </summary>
    public string? InlineArraySeparator { get; init; }

    /// <summary>
    /// Delegate to read the property value from an instance.
    /// Set this when using reflection-based metadata; source generators
    /// may provide their own strongly-typed accessor.
    /// </summary>
    public Func<object, object?>? Getter { get; init; }

    /// <summary>
    /// Delegate to write the property value on an instance.
    /// Set this when using reflection-based metadata; source generators
    /// may provide their own strongly-typed accessor.
    /// </summary>
    public Action<object, object?>? Setter { get; init; }

    /// <summary>
    /// Returns the current value of the property on <paramref name="obj"/>.
    /// </summary>
    public object? GetValue(object obj) => Getter!(obj);

    /// <summary>
    /// Sets the value of the property on <paramref name="obj"/>.
    /// </summary>
    public void SetValue(object obj, object? value) => Setter!(obj, value);
}

/// <summary>
/// Pre-computed metadata for a single constructor parameter on an INI-serializable type.
/// </summary>
/// <remarks>
/// All properties are <see langword="init"/>-only. Construct instances via
/// property-initializer syntax — no constructor is necessary.
/// </remarks>
public sealed class IniParameterInfo
{
    /// <summary>
    /// The CLR name of the parameter.
    /// </summary>
    public required string? Name { get; init; }

    /// <summary>
    /// The INI key name used to match this parameter.
    /// <see langword="null"/> when the parameter is marked <see cref="IniIgnoreAttribute"/>
    /// or has no matching property / <see cref="IniKeyAttribute"/>.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// The declared type of the parameter.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public required Type ParameterType { get; init; }

    /// <summary>
    /// The separator string from <see cref="IniInlineArrayAttribute"/>.
    /// <see langword="null"/> when the parameter is not marked with the attribute.
    /// </summary>
    public string? InlineArraySeparator { get; init; }

    /// <summary>
    /// <see langword="true"/> when the parameter has a default value.
    /// </summary>
    public bool HasDefaultValue { get; init; }

    /// <summary>
    /// The default value of the parameter, or <see langword="null"/> when none is defined.
    /// </summary>
    public object? DefaultValue { get; init; }
}
