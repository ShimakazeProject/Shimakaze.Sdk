namespace Shimakaze.Sdk.Models.Tmp;

public record struct TmpFile
{
    public FileHeader Header;
    public TileCellHeader[] TileCellHeaders;
}