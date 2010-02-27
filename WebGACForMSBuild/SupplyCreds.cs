using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using WebGAC.Core;

namespace WebGAC.MSBuild {
  public class SupplyCreds : Task {
    public override bool Execute() {
      Core.WebGAC gac = WebGACFactory.WebGAC;
      gac.CredRequestHandler = new CredentialsHelper().HandleCredentialsRequest;
      gac.Resolve("Dummy.Binary.For.ForcingResolution", new Version("2.0.0.0"), null, null, "Debug",
                  new string[] {"Debug"});

      return true;
    }
  }
}
