using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace WebGAC.MSBuild {
  public abstract class StoreTask : Task {
    private ITaskItem mAssemblyItem;
    private string mConfiguration;

    [Required]
    public ITaskItem AssemblyItem {
      get { return mAssemblyItem; }
      set { mAssemblyItem = value; }
    }

    /// <summary>
    /// The build configuration for the assembly.
    /// </summary>
    [Required]
    public string Configuration {
      get { return mConfiguration; }
      set { mConfiguration = value; }
    }

    /// <summary>
    /// Retrieves a list of paths for files that, if they exist, should be stored as well.
    /// </summary>
    protected IEnumerable<string> PossibleStorePaths {
      get {
        yield return AssemblyItem.ItemSpec;

        string itemExtension = Path.GetExtension(AssemblyItem.ItemSpec);
        string basePath = AssemblyItem.ItemSpec.Substring(0, AssemblyItem.ItemSpec.Length - itemExtension.Length);

        yield return basePath + ".pdb";
        yield return basePath + ".xml";
        yield return basePath + ".targets";
      }
    }
  }
}
