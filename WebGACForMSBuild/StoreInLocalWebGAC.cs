using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace WebGAC.MSBuild {
  /// <summary>
  /// Stores a given file into the local copy of the WebGAC.
  /// </summary>
  public class StoreInLocalWebGAC : StoreTask {
    public override bool Execute() {
      Assembly assembly = Assembly.Load(File.ReadAllBytes(AssemblyItem.ItemSpec));
//      string assemblyFullName = Path.GetFileNameWithoutExtension(AssemblyItem.ItemSpec);
      WebGAC.Core.WebGAC gac = WebGACFactory.WebGAC;

      foreach (string path in PossibleStorePaths) {
        if (File.Exists(path)) {
          gac.StoreLocal(assembly.FullName, path, Configuration);
        }
      }

      return true;
    }   
  }
}