﻿using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Mix File Header
/// </summary>
/// <param name="Files"> File Count <br /> it is the number of files in the archive. </param>
/// <param name="Size"> File Size <br /> it is the sum of all file sizes </param>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 6)]
public record struct MixMetadata(
    [field: FieldOffset(0)]
    short Files,
    [field: FieldOffset(sizeof(short))]
    int Size
);