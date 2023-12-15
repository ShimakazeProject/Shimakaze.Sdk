using System.Runtime.InteropServices;
using System.Text;

namespace Shimakaze.Sdk.Hva;

/// <summary>
/// HvaSectionName
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct HvaSectionName
{
    private unsafe fixed sbyte _characters[16];

    /// <inheritdoc/>
    public override unsafe string ToString()
    {
        fixed (sbyte* p = _characters)
            return new(p, 0, 16);
    }

    /// <summary>
    /// Create HvaSectionName From String
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">name cannot be longer that 16 byte.</exception>
    public static unsafe HvaSectionName Create(string name)
    {
        if (name is { Length: > 16 })
            throw new ArgumentException("name cannot be longer that 16 byte.");
        byte[] bytes = Encoding.UTF8.GetBytes(name);
        if (bytes is { Length: > 16 })
            throw new ArgumentException("name cannot be longer that 16 byte.");

        HvaSectionName sectionName = default;

        fixed (byte* ps = bytes)
            Buffer.MemoryCopy(ps, sectionName._characters, 16, 16);

        return sectionName;
    }
}
