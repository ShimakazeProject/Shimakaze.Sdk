using System.Collections;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Tests.Data.Csf;

[TestClass]
public class CsfDocumentTests
{
    [TestMethod]
    public void Test()
    {
        CsfDocument doc = new();
        Assert.IsNotNull(doc);
        Assert.IsFalse(doc.IsReadOnly);
        Assert.AreEqual(0, doc.Count);

        doc.Add("Label", new[] { new CsfValue("Value") });
        Assert.AreEqual("Label", doc[0].LabelName);
        doc[0] = new("Label1")
        {
            "Value1"
        };

        doc.Insert(0, new("0"));
        Assert.AreEqual("0", doc[0].LabelName);
        Assert.AreEqual("Label1", doc[1].LabelName);
        Assert.AreEqual(2, doc.Count);

        Assert.AreEqual(1, doc.IndexOf(doc[1]));

        CsfData[] array = new CsfData[2];
        doc.CopyTo(array, 0);
        Assert.AreEqual(2, array.Length);
        Assert.AreEqual("Label1", array[1].LabelName);

        using var e1 = doc.GetEnumerator();
        Assert.IsNotNull(e1);
        IEnumerator e2 = ((IEnumerable)doc).GetEnumerator();
        Assert.IsNotNull(e2);

        doc.RemoveAt(0);

        Assert.AreEqual(1, doc.Count);
        Assert.AreEqual("Label1", doc[0].LabelName);

        Assert.IsTrue(doc.Contains(doc[0]));

        Assert.IsTrue(doc.Remove(doc[0]));
        Assert.AreEqual(0, doc.Count);

        doc.Clear();
        Assert.AreEqual(0, doc.Count);
    }
}
