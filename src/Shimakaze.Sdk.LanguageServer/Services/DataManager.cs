using System.Collections.Concurrent;

using OmniSharp.Extensions.LanguageServer.Protocol;

using Shimakaze.Sdk.LanguageServer.Models;

namespace Shimakaze.Sdk.LanguageServer.Services;

/// <summary>
/// 数据管理器
/// </summary>
internal sealed class DataManager
{
    public ConcurrentDictionary<DocumentUri, DocumentContext> Context { get; private set; } = [];
}
