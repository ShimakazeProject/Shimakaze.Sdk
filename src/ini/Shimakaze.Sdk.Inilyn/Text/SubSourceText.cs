namespace Shimakaze.Sdk.Inilyn.Text;

internal sealed class SubSourceText(SourceText baseText, System.Range range)
    : SourceText(CalculateLineStarts(baseText.AsSpan()[range]), baseText.Uri)
{
    public override SourceText Root => baseText.Root;

    public override int Length { get; } = range.End.GetOffset(baseText.Length) - range.Start.GetOffset(baseText.Length);

    public override int StartIndex => baseText.StartIndex + range.Start.GetOffset(baseText.Length);

    public override int EndIndex => StartIndex + Length;

    public override char this[Index index] => AsSpan()[index];

    public override string ToString() => AsSpan().ToString();

    public override ReadOnlySpan<char> AsSpan() => baseText.AsSpan()[range];
}
