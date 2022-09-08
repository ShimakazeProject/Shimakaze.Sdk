using System.Diagnostics;

using Shimakaze.Sdk.Utils;

namespace Shimakaze.Sdk.Loader.Tmp.Test;

[TestClass]
public class TmpLoaderTest
{
    [TestMethod]
    public async Task ReadTest()
    {
        TmpLoader loader = new();

        await using FileStream fs = File.Open(Path.Combine("Assets", "clear01.tem"), FileMode.Open);
        var tmp = await loader.ReadAsync(fs, default);
        Console.WriteLine(tmp);
        tmp.TileCellHeaders.Each(o => Console.WriteLine(o));
    }
}