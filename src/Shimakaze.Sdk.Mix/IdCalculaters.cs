using System.IO.Hashing;
using System.Text;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// File Id Calculater
/// </summary>
public delegate uint IdCalculater(string name);

/// <summary>
/// File Id Calculaters
/// </summary>
public static class IdCalculaters
{
    /// <summary>
    /// Tiberian Sun Id Calc
    /// </summary>
    /// <param name="name"> File Name </param>
    /// <returns> Id </returns>
    public static uint TSIdCalculater(string name)
    {
        name = name.ToUpperInvariant();
        int l = name.Length;
        int a = l >> 2;
        if ((l & 3) is not 0)
        {
            name += (char)(byte)(l - (a << 2));
            int i = 3 - (l & 3);
            while (i-- is not 0)
                name += name[a << 2];
        }
        return BitConverter.ToUInt32(Crc32.Hash(Encoding.ASCII.GetBytes(name)), 0);
    }

    /// <inheritdoc cref="TDIdCalculater"/>
    [Obsolete("Use TDIdCalculater")]
    public static uint TDdCalculater(string name) => TDIdCalculater(name);

    /// <summary> 
    /// Id Calc for RA/TD 
    /// </summary> 
    /// <markup>
    /// This method are used by RedAlert and Tiberian Down. 
    /// </markup> 
    /// <param name="name">File Name</param> 
    ///  <returns>Id</returns>
    public static uint TDIdCalculater(string name)
    {
        name = name.ToUpperInvariant();
        int i = 0;
        uint id = 0;
        int l = name.Length;
        while (i < l)
        {
            uint a = 0; for (int j = 0; j < 4; j++)
            {
                a >>= 8;
                if (i < l)
                    a += ((uint)name[i]) << 24; i++;
            }
            id = (id << 1 | id >> 31) + a;
        }
        return id;
    }
}