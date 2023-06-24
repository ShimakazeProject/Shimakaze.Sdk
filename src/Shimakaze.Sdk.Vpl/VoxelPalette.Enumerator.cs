using System.Collections;

namespace Shimakaze.Sdk.Vpl;
public partial struct VoxelPalette
{
    private sealed class Enumerator : IEnumerator<VoxelPaletteSection>
    {
        private int _index = 0;
        private VoxelPalette _vpl;

        public Enumerator(VoxelPalette vpl)
        {
            _vpl = vpl;
        }

        public VoxelPaletteSection Current => _vpl[_index];

        object IEnumerator.Current => Current;

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            if (_index >= _vpl.Header.SectionCount)
                return false;

            _index++;
            return true;
        }
        public void Reset() => _index = 0;
    }
}
