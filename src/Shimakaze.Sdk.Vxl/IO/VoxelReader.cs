using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vxl;

namespace Shimakaze.Sdk.IO.Vxl;
/// <summary>
/// VoxelReader
/// </summary>
public sealed class VoxelReader : IReader<Voxel>
{
    private readonly byte[] _bytes;

    /// <summary>
    /// VoxelReader
    /// </summary>
    public VoxelReader(byte[] bytes)
    {
        _bytes = bytes;
    }
    /// <inheritdoc/>

    public Voxel Read()
    {
        Voxel voxel;
        unsafe
        {
            fixed (byte* p0 = _bytes)
            {
                voxel = new();
                byte* ptr = p0;

                voxel.Header = *(VoxelHeader*)ptr;
                ptr += sizeof(VoxelHeader);

                voxel.Palette = *(Palette*)ptr;
                ptr += sizeof(Palette);

                int limbDataOffset = sizeof(VoxelHeader) + sizeof(Palette) + voxel.Header.LimbsCount * sizeof(LimbHeader);

                // Headers
                voxel.LimbHeads = new LimbHeader[voxel.Header.LimbsCount];
                for (int i = 0; i < voxel.Header.LimbsCount; i++)
                {
                    voxel.LimbHeads[i] = *(LimbHeader*)ptr;
                    ptr += sizeof(LimbHeader);
                }

                // Tailers
                voxel.LimbTails = new LimbTailer[voxel.Header.LimbsCount];
                for (int i = 0; i < voxel.Header.LimbsCount; i++)
                {
                    ptr = p0 + limbDataOffset + voxel.Header.BodySize + i * sizeof(LimbTailer);
                    voxel.LimbTails[i] = *(LimbTailer*)ptr;
                    ptr += sizeof(LimbTailer);
                }

                voxel.LimbBodies = new LimbBody[voxel.Header.LimbsCount];
                for (int i = 0; i < voxel.Header.LimbsCount; i++)
                {
                    int n = voxel.LimbTails[i].Size.X * voxel.LimbTails[i].Size.Y;

                    int* pSpanStart = (int*)(p0 + limbDataOffset + voxel.LimbTails[i].SpanStartOffset);
                    int* pSpanEnd = (int*)(p0 + limbDataOffset + voxel.LimbTails[i].SpanEndOffset);
                    byte* pData = p0 + limbDataOffset + voxel.LimbTails[i].SpanDataOffset;

                    voxel.LimbBodies[i] = new()
                    {
                        SpanStart = new int[n],
                        SpanEnd = new int[n],
                        Data = new SpanStruct[n],
                    };
                    for (int j = 0; j < n; j++)
                    {
                        voxel.LimbBodies[i].SpanStart[j] = pSpanStart[j];
                        voxel.LimbBodies[i].SpanEnd[j] = pSpanEnd[j];
                        byte* start = pData + voxel.LimbBodies[i].SpanStart[j];
                        byte* end = pData + voxel.LimbBodies[i].SpanStart[j];

                        byte* p = start;
                        voxel.LimbBodies[i].Data[j].SkipCount = *p++;
                        voxel.LimbBodies[i].Data[j].NumVoxels = *p++;
                        voxel.LimbBodies[i].Data[j].Voxels = new SpanVoxel[voxel.LimbBodies[i].Data[j].NumVoxels];
                        int size = sizeof(SpanVoxel) * voxel.LimbBodies[i].Data[j].NumVoxels;

                        fixed (SpanVoxel* dist = voxel.LimbBodies[i].Data[j].Voxels)
                            Buffer.MemoryCopy(p, dist, size, size);

                        p += size;
                        voxel.LimbBodies[i].Data[j].NumVoxels2 = *p++;
                    }
                }
            }
        }
        return voxel;
    }
}
