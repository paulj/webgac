using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace WebGAC.Core.Test {
  [TestFixture]
  public class WebGACConfigTest {
    [Test]
    public void ShouldSendToMultipleDelegates() {
      WebGACConfig config = new WebGACConfig();
      bool gotA = false;
      bool gotB = false;

      config.RepositoriesUpdated += delegate { gotA = true; };
      config.RepositoriesUpdated += delegate { gotB = true; };

      config.Repositories = new string[0];

      Assert.IsTrue(gotA);
      Assert.IsTrue(gotB);
    }
  }
}
