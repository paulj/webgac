using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace WebGAC.MSBuild {
  /// <summary>
  /// Stores a given file into the remote copy of the WebGAC.
  /// </summary>
  public class StoreInRemoteWebGAC : StoreTask {
    private string mRepository;

    public override bool Execute() {
      Assembly assembly = Assembly.Load(File.ReadAllBytes(AssemblyItem.ItemSpec));
//      string assemblyFullName = Path.GetFileNameWithoutExtension(AssemblyItem.ItemSpec);
      WebGAC.Core.WebGAC gac = WebGACFactory.WebGAC;
      foreach (string path in PossibleStorePaths) {
        if (File.Exists(path)) {
          gac.StoreRemote(assembly.FullName, path, Configuration, mRepository);
        }
      }

      return true;
    }

    [Required]
    public string Repository {
      get { return mRepository; }
      set { mRepository = value; }
    }
  }
}
