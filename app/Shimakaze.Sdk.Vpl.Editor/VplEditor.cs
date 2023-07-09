using Sharprompt;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl.Editor;

internal sealed class VplEditor
{
    private const int X_SECTION_OFFSET = 3 * 16 + 4;
    private const int Y_OFFSET = 1;
    private const int SIZE_OF_CELL = 3;
    public readonly VoxelPalette Vpl;
    private readonly Palette _pal;
    private readonly Func<VplEditor, Task> _saver;
    private bool _isEditing;
    private (int X, int Y, int Section) _current;
    private (int X, int Y) _editing;

    public VplEditor(VoxelPalette vpl, Palette pal, Func<VplEditor, Task> saver)
    {
        Vpl = vpl;
        _pal = pal;
        _saver = saver;
    }

    public async Task RunAsync()
    {
        PrintColor();
        while (true)
        {
            if (_isEditing)
                Console.SetCursorPosition(_editing.X * SIZE_OF_CELL, Y_OFFSET + _editing.Y);
            else
                Console.SetCursorPosition(X_SECTION_OFFSET + _current.X * SIZE_OF_CELL, Y_OFFSET + _current.Y);

            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Tab when !_isEditing && key.Modifiers is ConsoleModifiers.Shift && _current.Section > 0:
                case ConsoleKey.N when !_isEditing && _current.Section > 0:
                    _current.Section--;
                    PrintColor();
                    break;
                case ConsoleKey.Tab when !_isEditing && key.Modifiers is 0 && _current.Section + 1 < Vpl.Header.SectionCount:
                case ConsoleKey.M when !_isEditing && _current.Section + 1 < Vpl.Header.SectionCount:
                    _current.Section++;
                    PrintColor();
                    break;
                case ConsoleKey.LeftArrow when !_isEditing && _current.X > 0:
                case ConsoleKey.H when !_isEditing && _current.X > 0:
                    _current.X--;
                    break;
                case ConsoleKey.RightArrow when !_isEditing && _current.X < 15:
                case ConsoleKey.L when !_isEditing && _current.X < 15:
                    _current.X++;
                    break;
                case ConsoleKey.UpArrow when !_isEditing && _current.Y > 0:
                case ConsoleKey.K when !_isEditing && _current.Y > 0:
                    _current.Y--;
                    break;
                case ConsoleKey.DownArrow when !_isEditing && _current.Y < 15:
                case ConsoleKey.J when !_isEditing && _current.Y < 15:
                    _current.Y++;
                    break;
                case ConsoleKey.LeftArrow when _isEditing && _editing.X > 0:
                case ConsoleKey.H when _isEditing && _editing.X > 0:
                    _editing.X--;
                    break;
                case ConsoleKey.RightArrow when _isEditing && _editing.X < 15:
                case ConsoleKey.L when _isEditing && _editing.X < 15:
                    _editing.X++;
                    break;
                case ConsoleKey.UpArrow when _isEditing && _editing.Y > 0:
                case ConsoleKey.K when _isEditing && _editing.Y > 0:
                    _editing.Y--;
                    break;
                case ConsoleKey.DownArrow when _isEditing && _editing.Y < 15:
                case ConsoleKey.J when _isEditing && _editing.Y < 15:
                    _editing.Y++;
                    break;
                case ConsoleKey.Enter when !_isEditing:
                case ConsoleKey.Spacebar when !_isEditing:
                    byte index = Vpl[_current.Section][(_current.Y * 16) + _current.X];
                    _editing.X = index % 16;
                    _editing.Y = index / 16;
                    _isEditing = true;
                    PrintColor();
                    break;
                case ConsoleKey.Enter when _isEditing:
                case ConsoleKey.Spacebar when _isEditing:
                    index = (byte)((_editing.Y * 16) + _editing.X);
                    {
                        var tmp = Vpl[_current.Section];
                        tmp[(_current.Y * 16) + _current.X] = index;
                        Vpl[_current.Section] = tmp;
                    }
                    _isEditing = false;
                    PrintColor();
                    break;
                case ConsoleKey.Escape when _isEditing:
                case ConsoleKey.Q when _isEditing:
                    _isEditing = false;
                    PrintColor();
                    break;
                case ConsoleKey.Escape when !_isEditing:
                case ConsoleKey.Q when !_isEditing:
                    Console.SetCursorPosition(0, Y_OFFSET + 16);
                    var isExit = Prompt.Confirm("Are you sure you want to exit? Your changed will NOT saved!", false);
                    if (isExit)
                        Environment.Exit(0);

                    PrintColor();
                    break;
                case ConsoleKey.S:
                    Console.SetCursorPosition(0, Y_OFFSET + 16);
                    await _saver(this);
                    PrintColor();
                    break;
            }
        }
    }

    void PrintColor()
    {
        Console.CursorVisible = false;
        Console.Clear();
        Console.SetCursorPosition(0, 0);
        Console.Write("色板：");
        Console.SetCursorPosition(X_SECTION_OFFSET, 0);
        Console.Write("Section: #");
        Console.Write((_current.Section + 1).ToString("D2"));
        Console.WriteLine("\x1B[0m");
        for (int y = 0; y < 256 / 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Console.Write(_pal[(y * 16) + x].GetANSIString(true));
                Console.Write("   ");
            }

            Console.Write("\x1B[0m    ");

            for (int x = 0; x < 16; x++)
            {
                Console.Write(_pal[Vpl[_current.Section][(y * 16) + x]].GetANSIString(true));
                if (x == _current.X && y == _current.Y && _isEditing)
                {
                    Console.Write(_pal[Vpl[_current.Section][(y * 16) + x]].GetReverse().GetANSIString(false));
                    Console.Write(" x ");
                }
                else
                    Console.Write("   ");
            }

            Console.WriteLine("\x1B[0m");
        }
        Console.CursorVisible = true;
    }
}
