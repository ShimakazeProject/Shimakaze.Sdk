using System.Globalization;
using System.Text;

namespace Shimakaze.Sdk.Engine.Cli.TUI;

internal record ShortKey(ConsoleKey Key, ConsoleModifiers Modifiers)
{
    public static implicit operator ShortKey(ConsoleKeyInfo keyInfo) => new(keyInfo.Key, keyInfo.Modifiers);
};
internal record NamedShortKey(ConsoleKey Key, ConsoleModifiers Modifiers, string Name) : ShortKey(Key, Modifiers)
{
    public int Order { get; init; }
    public virtual string Name { get; } = Name;
}
internal sealed record NamedSwitchShortKey(ConsoleKey Key, ConsoleModifiers Modifiers, Func<bool, string> GetName, Func<bool> GetStatus) : NamedShortKey(Key, Modifiers, GetName(GetStatus()))
{
    public override string Name => GetName(GetStatus());
}

internal sealed class ShortKeyManager
{
    private const string NormalKey = "\e[7m{0}\e[0m {1}";
    private const string ActivatedKey = "\e[7m\e[92m{0}\e[0m {1}";
    private static readonly CompositeFormat NormalKeyFormat = CompositeFormat.Parse(NormalKey);
    private static readonly CompositeFormat ActivatedKeyFormat = CompositeFormat.Parse(ActivatedKey);

    private readonly List<Dictionary<ShortKey, Action>> _layers = [];
    public int CurrentLayer { get; set; }

    public void Regist(int layer, ShortKey shortKey, Action action)
    {
        while (layer >= _layers.Count)
            _layers.Add([]);

        var currentLayer = _layers[layer];
        currentLayer[shortKey] = action;
    }

    public void Receive(in ConsoleKeyInfo shortKey)
    {
        var currentLayer = _layers[CurrentLayer];
        foreach (var item in currentLayer)
        {
            if (item.Key.Modifiers != shortKey.Modifiers)
                continue;
            if (item.Key.Key != shortKey.Key)
                continue;

            item.Value();
            return;
        }
    }

    public void RenderField(TextWriter writer, int fieldWidth, int fieldPerLine, int take)
    {
        NamedShortKey[] fields = [.. _layers[CurrentLayer]
            .Select(i => i.Key)
            .OfType<NamedShortKey>()
            .OrderByDescending(i => i.Order)
            .Take(take)];

        for (int i = 0; i < fields.Length; i++)
        {
            if (i is not 0 && (i % fieldPerLine) is 0)
                writer.WriteLine();

            ReadOnlySpan<char> field = RenderField(fields[i]);
            writer.Write(field);
            var len = NanoFramework.GetDisplayWidth(field);
            var size = fieldWidth - len;
            for (int j = 0; j < size; j++)
                writer.Write(' ');
        }
    }

    private static string RenderField(NamedShortKey key)
    {
        CompositeFormat format = NormalKeyFormat;
        if (key is NamedSwitchShortKey switched && switched.GetStatus())
            format = ActivatedKeyFormat;

        StringBuilder code = new();
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
            code.Append('^');
        if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
            code.Append("S-");
        if (key.Modifiers.HasFlag(ConsoleModifiers.Alt))
            code.Append("M-");
        code.Append(key.Key switch
        {
            ConsoleKey.Spacebar => "Space",
            ConsoleKey.LeftArrow => "←",
            ConsoleKey.UpArrow => "↑",
            ConsoleKey.RightArrow => "→",
            ConsoleKey.DownArrow => "↓",
            _ => key.Key.ToString(),
        });
        if (code.Length < 2)
            code.Insert(0, ' ');
        if (code.Length < 3)
            code.Append(' ');

        return string.Format(CultureInfo.InvariantCulture, format, code, key.Name);
    }

    public void MeasureField(out int count, out int width)
    {
        var keys = _layers[CurrentLayer]
            .Select(i => i.Key)
            .OfType<NamedShortKey>();

        count = keys.Count();
        width = keys.Max(MeasureField);
    }

    private int MeasureField(NamedShortKey key)
    {
        int len = 0;

        if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
            len++;
        if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
            len += 2;
        if (key.Modifiers.HasFlag(ConsoleModifiers.Alt))
            len += 2;
        len += key.Key.ToString().Length;
        len = int.Max(len, 2);

        len++;
        len += key.Name.Length;

        len += 2;
        return len;
    }
}
