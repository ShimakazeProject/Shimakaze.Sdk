﻿using System.Text.Json;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Json.Converter.V2;

namespace Shimakaze.Sdk.IO.Csf.Json;

/// <summary>
/// CsfJsonV2Reader.
/// </summary>
public sealed class CsfJsonV2Reader : AsyncReader<CsfDocument>, IDisposable, IAsyncDisposable
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="options"> </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public CsfJsonV2Reader(Stream stream, JsonSerializerOptions? options = null, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            options.Converters.Add(item);

        _options = options;
    }

    /// <inheritdoc />
    public CsfDocument Deserialize()
    {
        return JsonSerializer.Deserialize<CsfDocument>(BaseStream, _options);
    }

    /// <inheritdoc />
    public override async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<CsfDocument>(BaseStream, _options, cancellationToken);
    }
}