using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Shimakaze.Sdk.Csf.Json;

internal static class JsonSerializerOptionsExtensions
{
    extension(JsonSerializerOptions options)
    {
        public bool TryGetTypeInfo<T>([NotNullWhen(true)] out JsonTypeInfo<T>? typeInfo)
        {
            typeInfo = default;
            if (!options.TryGetTypeInfo(typeof(T), out var jsonTypeInfo))
                return false;

            typeInfo = jsonTypeInfo as JsonTypeInfo<T>;
            return typeInfo is not null;
        }

        public bool TryGetConverter<TConverter>([NotNullWhen(true)] out TConverter? converter)
            where TConverter : JsonConverter
        {
            converter = options.Converters.FirstOrDefault(i => i is TConverter) as TConverter;
            return converter is not null;
        }

        public bool TryGetConverter<T, TConverter>(Expression<Func<T, object?>> propertySelector, [NotNullWhen(true)] out TConverter? converter)
            where TConverter : JsonConverter
        {
            converter = default;
            if (!options.TryGetTypeInfo<T>(out var typeInfo))
                return false;

            string propertyName = propertySelector.Body switch
            {
                MemberExpression member => member.Member.Name,

                UnaryExpression unary when unary.Operand is MemberExpression member =>
                    member.Member.Name,

                _ => throw new ArgumentException(
                    "Expression must be a property selector.",
                    nameof(propertySelector))
            };

            propertyName = options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
            if (typeInfo.Properties.FirstOrDefault(p => p.Name == propertyName) is not { } property)
                return false;

            converter = property.CustomConverter as TConverter;
            return converter is not null;
        }
    }
}
