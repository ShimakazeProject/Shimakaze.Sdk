using System.IO.Hashing;
using System.Text;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// File Id Calculator
/// </summary>
public delegate uint IdCalculator(string name, Encoding? encoding = default);

/// <summary>
/// File Id Calculators
/// </summary>
public static class IdCalculators
{
    /// <summary>
    /// Tiberian Sun Id Calc
    /// </summary>
    /// <param name="name"> File Name </param>
    /// <param name="encoding"> File Name Encoding </param>
    /// <returns> Id </returns>
    public static uint TSIdCalculator(string name, Encoding? encoding = default)
    {
        encoding ??= Encoding.GetEncoding(0);
        name = name.ToUpperInvariant();
        List<byte> data = [.. encoding.GetBytes(name)];
        int l = data.Count;
        int a = l >> 2;
        if ((l & 3) is not 0)
        {
            data.Add((byte)(l - (a << 2)));
            int i = 3 - (l & 3);
            while (i-- is not 0)
                data.Add(data[a << 2]);

        }
        return BitConverter.ToUInt32(Crc32.Hash([.. data]), 0);
    }

    /// <summary> 
    /// Id Calc for RA/TD 
    /// </summary> 
    /// <markup>
    /// This method are used by RedAlert and Tiberian Down. 
    /// </markup> 
    /// <param name="name">File Name</param> 
    /// <param name="encoding"> File Name Encoding </param>
    ///  <returns>Id</returns>
    public static uint TDIdCalculator(string name, Encoding? encoding = default)
    {
        encoding ??= Encoding.GetEncoding(0);
        name = name.ToUpperInvariant();
        var data = encoding.GetBytes(name);
        int i = 0;
        uint id = 0;
        int l = data.Length;
        while (i < l)
        {
            uint a = 0; for (int j = 0; j < 4; j++)
            {
                a >>= 8;
                if (i < l)
                {
                    a += ((uint)data[i]) << 24;
                }

                i++;
            }
            id = ((id << 1) | (id >> 31)) + a;
        }
        return id;
    }
}
