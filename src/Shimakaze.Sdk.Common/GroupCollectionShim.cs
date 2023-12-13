using System.Text.RegularExpressions;

namespace Shimakaze.Sdk;

/// <summary>
/// 垫片
/// </summary>
public static class GroupCollectionShim
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public static IEnumerable<Group> GetValues(this GroupCollection group)
    {
#if NETSTANDARD
        return group.Cast<Group>();
#else
        return group.Values; 
#endif
    }
}
