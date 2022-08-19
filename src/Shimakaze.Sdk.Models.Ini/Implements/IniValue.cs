using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Models.Ini.implements;
[DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
public sealed class IniValue
{
    internal string Raw;

    /// <summary>
    /// if (this is Not Empty) return true; <br /> else return false
    /// </summary>

    public bool HasData => !string.IsNullOrEmpty(Raw);

    public IniValue(string value) => Raw = value;

    /// <summary>
    /// IgoneCase <br /> Y(es), T(ure) 1 return true <br /> N(o), F(alse) 0 return false <br />
    /// else throw FormatException
    /// </summary>
    public static explicit operator bool(IniValue value) =>
        value.Raw is not null
        && ((new char[] { 'y', 'Y', 't', 'T', '1' }).Contains(value.Raw[0])
        || ((new char[] { 'n', 'N', 'f', 'F', '0' }).Contains(value.Raw[0])
        ? false : throw new FormatException($"{value.Raw} is not bool")));

    public static explicit operator byte(IniValue value) => byte.Parse(value.Raw);

    public static explicit operator decimal(IniValue value) => decimal.Parse(value.Raw);

    public static explicit operator double(IniValue value) => double.Parse(value.Raw);

    public static explicit operator float(IniValue value) => float.Parse(value.Raw);

    public static explicit operator int(IniValue value) => int.Parse(value.Raw);

    public static explicit operator long(IniValue value) => long.Parse(value.Raw);

    public static explicit operator sbyte(IniValue value) => sbyte.Parse(value.Raw);

    public static explicit operator short(IniValue value) => short.Parse(value.Raw);

    public static explicit operator uint(IniValue value) => uint.Parse(value.Raw);

    public static explicit operator ulong(IniValue value) => ulong.Parse(value.Raw);

    public static explicit operator ushort(IniValue value) => ushort.Parse(value.Raw);

    public static implicit operator IniValue(string s) => new(s);

    public static implicit operator IniValue(int i) => new(i.ToString());

    public static implicit operator IniValue(long i) => new(i.ToString());

    public static implicit operator IniValue(bool i) => new(i.ToString());

    public static implicit operator IniValue(double d) => new(d.ToString());

    public static implicit operator IniValue(decimal i) => new(i.ToString());

    public static implicit operator string(IniValue value) => value.ToString();

    public override bool Equals(object? obj) => Raw.Equals((obj as IniValue)?.Raw);

    public override int GetHashCode() => Raw.GetHashCode();

    public override string ToString() => Raw;

    public static bool operator ==(IniValue left, IniValue right) => left.Equals(right);

    public static bool operator !=(IniValue left, IniValue right) => !(left == right);
}
