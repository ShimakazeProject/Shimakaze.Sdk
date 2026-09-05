using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// YAML 类型转换器基类
/// </summary>
public abstract class YamlConverter : IYamlTypeConverter
{
    /// <summary>
    /// 获取转换器支持类型
    /// </summary>
    public abstract Type Type { get; }

    /// <inheritdoc/>
    public abstract bool Accepts(Type type);

    /// <inheritdoc/>
    public abstract object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer);

    /// <inheritdoc/>
    public abstract void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer);
}

/// <summary>
/// YAML 类型转换器基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class YamlConverter<T> : YamlConverter
{
    /// <inheritdoc/>
    public sealed override Type Type => typeof(T);

    /// <inheritdoc/>

    public sealed override bool Accepts(Type type)
        => type == typeof(T);

    /// <inheritdoc/>
    public abstract T? Read(
        IParser parser,
        ObjectDeserializer deserializer,
        YamlSerializerOptions options);

    /// <inheritdoc/>
    public abstract void Write(
        IEmitter emitter,
        T value,
        ObjectSerializer serializer,
        YamlSerializerOptions options);


    /// <inheritdoc/>
    public override object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        => Read(parser, rootDeserializer, new YamlSerializerOptions());

    /// <inheritdoc/>
    public override void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        => Write(emitter, (T)value!, serializer, new YamlSerializerOptions());
}
