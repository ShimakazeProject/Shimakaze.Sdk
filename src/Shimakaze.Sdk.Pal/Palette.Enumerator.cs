using System.Collections;

namespace Shimakaze.Sdk.Pal;

public partial struct Palette
{
    private sealed unsafe class Enumerator : IEnumerator<Color>
    {
        private int _index = 0;
        private Palette _colors;

        public Enumerator(Palette colors)
        {
            _colors = colors;
        }

        public Color Current => _colors[_index];

        object IEnumerator.Current => Current;

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            if (_index is not < COLOR_COUNT)
                return false;

            _index++;
            return true;
        }
        public void Reset() => _index = 0;
    }
}
