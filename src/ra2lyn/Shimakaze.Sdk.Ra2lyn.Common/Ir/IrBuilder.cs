using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Ir;

/// <summary>
/// IR 构建器：按命令次序产出确定性的实体与属性，并据此分配 <see cref="EntityId"/> 与 <see cref="PropertyId"/>。
/// </summary>
/// <remarks>
/// 前端与测试都只通过命令构造 IR：编号由命令顺序决定，因此同一份源文本的编号始终稳定。
/// </remarks>
public sealed class IrBuilder
{
    private readonly List<ICommand> _commands = [];
    private int _entityCount;
    private int _propertyCount;

    /// <summary>
    /// 登记一条实体命令。
    /// </summary>
    /// <param name="name">实体名。</param>
    /// <param name="properties">实体属性。</param>
    /// <param name="origin">实体在源文件中的位置。</param>
    /// <param name="baseNames">继承/混合名称。</param>
    public EntityCommand AddEntity(
        string name,
        IEnumerable<PropertyCommand>? properties = null,
        SourceSpan origin = default,
        IEnumerable<string>? baseNames = null)
    {
        EntityCommand command = new()
        {
            Name = name,
            Properties = properties is null ? [] : [.. properties],
            Origin = origin,
            BaseNames = baseNames is null ? [] : [.. baseNames],
        };
        _commands.Add(command);
        return command;
    }

    /// <summary>
    /// 登记一条诊断（例如前端在降级阶段产生的诊断），最终随 <see cref="FrontendResult"/> 返回。
    /// </summary>
    public void Report(Diagnostic diagnostic)
        => _commands.Add(new DiagnosticCommand { Diagnostic = diagnostic });

    /// <summary>
    /// 汇总所有命令产物，编号在此时按命令顺序分配。
    /// </summary>
    /// <param name="group">front 结果所属的规则组。</param>
    /// <param name="sources">前端读取的源文件。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public FrontendResult Build(string group, IReadOnlyList<SourceFile> sources, CancellationToken cancellationToken = default)
    {
        List<EntityModel> entities = [];
        List<Diagnostic> diagnostics = [];

        foreach (var command in _commands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (command)
            {
                case EntityCommand entity:
                    entities.Add(CreateEntity(entity, group));
                    break;
                case DiagnosticCommand diagnostic:
                    diagnostics.Add(diagnostic.Diagnostic);
                    break;
            }
        }

        return new FrontendResult(group, sources, entities, diagnostics);
    }

    /// <summary>
    /// 构造单个实体，并分配其实体标识与属性标识。
    /// </summary>
    /// <param name="command">实体命令。</param>
    /// <param name="group">实体所属的规则组。</param>
    public EntityModel CreateEntity(EntityCommand command, string group)
    {
        EntityId entityId = new(_entityCount++);

        List<PropertyModel> properties = [];
        foreach (var property in command.Properties)
        {
            properties.Add(new PropertyModel
            {
                Id = new(_propertyCount++),
                Key = property.Key,
                Value = property.Value,
                ListSeparator = property.ListSeparator,
                Origin = property.Origin,
            });
        }

        return new EntityModel
        {
            Id = entityId,
            Group = command.Group ?? group,
            Name = command.Name,
            BaseNames = [.. command.BaseNames],
            Properties = properties,
            Origin = command.Origin,
        };
    }

    /// <summary>
    /// IR 命令基类型。
    /// </summary>
    public abstract class ICommand;

    /// <summary>
    /// 实体命令。
    /// </summary>
    public sealed class EntityCommand : ICommand
    {
        /// <summary>
        /// 实体名。
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// 实体所属的规则组；为 <see langword="null"/> 时使用构建时的默认组。
        /// </summary>
        public string? Group { get; init; }

        /// <summary>
        /// 实体属性。
        /// </summary>
        public required List<PropertyCommand> Properties { get; init; }

        /// <summary>
        /// 实体在源文件中的位置。
        /// </summary>
        public SourceSpan Origin { get; init; }

        /// <summary>
        /// 继承/混合名称。
        /// </summary>
        public required List<string> BaseNames { get; init; }
    }

    /// <summary>
    /// 属性命令。
    /// </summary>
    public sealed class PropertyCommand
    {
        /// <summary>
        /// 键名。
        /// </summary>
        public required string Key { get; init; }

        /// <summary>
        /// 原始值。
        /// </summary>
        public required string Value { get; init; }

        /// <summary>
        /// 列表分隔符。
        /// </summary>
        public string? ListSeparator { get; init; }

        /// <summary>
        /// 属性在源文件中的位置。
        /// </summary>
        public SourceSpan Origin { get; init; }
    }

    private sealed class DiagnosticCommand : ICommand
    {
        public required Diagnostic Diagnostic { get; init; }
    }
}