namespace Shimakaze.Sdk.Inilyn.Models.Symbol;
/// <summary>
/// 表示一个符号（Symbol），是所有符号对象（节、键等）的基类。
/// </summary>
public abstract class Symbol
{
    /// <summary>
    /// 获取该符号的名称。
    /// </summary>
    public abstract string Name { get; }
}
