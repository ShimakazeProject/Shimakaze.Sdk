using System.Collections;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Tests.Data.Csf;

[TestClass]
public class CsfDataTests
{
    [TestMethod]
    public void Test()
    {
        CsfData data = new();
        Assert.IsNotNull(data);
        Assert.IsFalse(data.IsReadOnly);

        data = new("Label");
        Assert.AreEqual("Label", data.LabelName);
        Assert.AreEqual("Label".Length, data.LabelName.Length);

        CsfValue value1 = new("Hello");
        CsfValue value2 = new("World");
        data = new("Label", new[] { value1 });
        Assert.AreEqual("Label", data.LabelName);
        Assert.AreEqual("Label".Length, data.LabelName.Length);
        Assert.AreEqual(1, data.StringCount);
        Assert.AreEqual(1, data.Count);
        Assert.AreEqual("Hello", data[0].Value);
        data[0].Value = "A";
        Assert.AreEqual("A", data[0].Value);
        data[0].Value = "Hello";
        data[0] = value2;
        Assert.AreEqual("World", data[0].Value);

        data.Insert(0, value1);
        Assert.AreEqual("Hello", data[0].Value);
        Assert.AreEqual("World", data[1].Value);
        Assert.AreEqual(1, data.StringCount);
        Assert.AreEqual(2, data.Count);
        data.ReCount();
        Assert.AreEqual(2, data.StringCount);

        Assert.AreEqual(1, data.IndexOf(value2));

        data.RemoveAt(0);
        Assert.AreEqual(1, data.Count);
        Assert.AreEqual("World", data[0].Value);

        data.Add("value1");
        Assert.AreEqual(2, data.Count);
        Assert.AreEqual("value1", data[1].Value);

        data.Add("value2", "extra");
        Assert.AreEqual(3, data.Count);
        Assert.AreEqual("value2", data[2].Value);
        Assert.AreEqual("extra", (data[2] as CsfValueExtra)?.ExtraValue);

        Assert.IsTrue(data.Contains(value2));

        Assert.IsTrue(data.Remove(value2));
        Assert.AreEqual(2, data.Count);

        CsfValue[] array = new CsfValue[2];
        data.CopyTo(array, 0);
        Assert.AreEqual(2, array.Length);
        Assert.AreEqual("value1", array[0].Value);
        using var e1 = data.GetEnumerator();
        Assert.IsNotNull(e1);
        IEnumerator e2 = ((IEnumerable)data).GetEnumerator();
        Assert.IsNotNull(e2);

        data.Clear();
        Assert.AreEqual(0, data.Count);
    }
}
