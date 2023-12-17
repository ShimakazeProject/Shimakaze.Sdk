using System.Text.RegularExpressions;

namespace Shimakaze.Sdk;

/// <summary>
/// GroupCollection 实用工具
/// </summary>
internal static class GroupCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public static IEnumerable<Group> GetValues(this GroupCollection group)
    {
#if NETSTANDARD || NETFRAMEWORK
        return group.Cast<Group>();
#else
        return group.Values; 
#endif
    }
}
