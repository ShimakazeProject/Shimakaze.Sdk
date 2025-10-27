namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VoxelSpanSegment
/// </summary>
public sealed record class VoxelSpanSegment(byte SkipCount, byte NumVoxels, Memory<Voxel> Voxels, byte NumVoxels2)
{
    /// <summary>
    /// Number of empty voxels before this span segment
    /// </summary>
    public byte SkipCount { get; set; } = SkipCount;
    /// <summary>
    /// Number of voxels in this span segment
    /// </summary>
    public byte NumVoxels { get; set; } = NumVoxels;

    /// <summary>
    /// The voxels in the span segment
    /// </summary>
    public Memory<Voxel> Voxels { get; set; } = Voxels;

    /// <summary>
    /// Always equal to <see cref="NumVoxels" />
    /// </summary>
    public byte NumVoxels2 { get; set; } = NumVoxels2;

    /// <summary>
    /// Reads a <see cref="VoxelSpanSegment" /> from a <see cref="Stream" />
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="EndOfStreamException"></exception>
    /// <exception cref="FormatException"></exception>
    public static VoxelSpanSegment ReadFrom(Stream stream)
    {
        var skipCount = stream.ReadByte();
        if (skipCount is -1)
            throw new EndOfStreamException();

        var numVoxels = stream.ReadByte();
        if (skipCount is -1)
            throw new EndOfStreamException();

        Memory<Voxel> voxels = new Voxel[numVoxels];
        if (numVoxels is > 0)
            stream.Read(voxels);

        var numVoxels2 = stream.ReadByte();
        if (numVoxels2 is -1)
            throw new EndOfStreamException();

        if (numVoxels != numVoxels2)
            throw new FormatException("NumVoxels are not equal than NumVoxels2");

        return unchecked(new((byte)skipCount, (byte)numVoxels, voxels, (byte)numVoxels2));
    }

    /// <summary>
    /// Write the <see cref="VoxelSpanSegment"/> to the <paramref name="stream"/>
    /// </summary>
    /// <param name="stream"></param>
    public void WriteTo(Stream stream)
    {
        stream.WriteByte(SkipCount);
        stream.WriteByte(NumVoxels);
        stream.Write(Voxels);
        stream.WriteByte(NumVoxels2);
    }
}
