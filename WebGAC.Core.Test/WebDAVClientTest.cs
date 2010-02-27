using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace WebGAC.Core.Test {
  [TestFixture]
  public class WebDAVClientTest {
    [Test]
    public void GetCollectionHierarchyInReverseShouldReturnValidList() {
      WebDAVClient client = new WebDAVClient("http://localhost", new WebDAVCache(GetLocationForCacheFile, "content.xml"), null, null);
      using (IEnumerator<string> hierarchy = client.GetCollectionHierarchyInReverse("Test/0.1_Debug").GetEnumerator()) {
        Assert.IsTrue(hierarchy.MoveNext());
        Assert.AreEqual("Test/0.1_Debug/", hierarchy.Current);
        Assert.IsTrue(hierarchy.MoveNext());
        Assert.AreEqual("Test/", hierarchy.Current);
        Assert.IsFalse(hierarchy.MoveNext());
      }
    }

    private string GetLocationForCacheFile(string pCollection) {
      return pCollection.Replace('/', Path.DirectorySeparatorChar);
    }
  }
}
