using System;
using System.Collections.Generic;
using System.Text;
using WebGAC.Core;

namespace WebGAC.MSBuild {
  public class WebGACFactory {
    public static WebGAC.Core.WebGAC WebGAC {
      get {
        Core.WebGAC result = new Core.WebGAC();
        result.CredRequestHandler = new CredentialsHelper().HandleReadOnlyCredentialsRequest;

        return result;
      }
    }
  }
}
