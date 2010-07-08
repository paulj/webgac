using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using WebGAC.Core;

namespace WebGAC.MSBuild {
  /// <summary>
  /// Adds WebGAC folders to the assembly search path for any WebGAC references.
  /// </summary>
  public class AddWebGACAssemblySearchPaths : Task {
    /// <summary>
    /// String used in the AssemblySearchPath property to specify where
    /// WebGAC folder items should be added.
    /// </summary>
    public const string WebGACSearchPathIndicator = "{WebGAC}";

    private string[] mPaths;
    private ITaskItem[] mAssemblies;
    private string mPrimaryConfiguration;
    private string mAllConfigurationsStr;
    private string[] mAllConfigurations;
    private readonly WebGAC.Core.WebGAC mGac;

    public AddWebGACAssemblySearchPaths() {
      mGac = WebGACFactory.WebGAC;
    }

    /// <summary>
    /// Gets or sets the Mono assembly search paths.
    /// </summary>
    [Required]
    [Output]
    public string[] Paths {
      get { return mPaths; }
      set { mPaths = value; }
    }

    /// <summary>
    /// These are the assembly references in the project being built.  This
    /// set of items is also passed to the ResolveAssemblyReference task.
    /// </summary>
    [Required]
    public ITaskItem[] Assemblies {
      get { return mAssemblies; }
      set { mAssemblies = value; }
    }

    /// <summary>
    /// The current build configuration.
    /// </summary>
    public string PrimaryConfiguration {
      get { return mPrimaryConfiguration; }
      set { mPrimaryConfiguration = value; }
    }

    /// <summary>
    /// The list of all available configurations.
    /// </summary>
    [Required]
    public string AllConfigurations {
      get { return mAllConfigurationsStr; }
      set {
        mAllConfigurationsStr = value;
        mAllConfigurations = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      }
    }

    /// <summary>
    /// Replaces the {WebGAC} entry in the AssemblySearchPaths.
    /// </summary>
    public override bool Execute() {
      List<string> updatedSearchPaths = new List<string>();
      IDictionary<string, object> webGACSearchPaths = new Dictionary<string, object>();

      // Default the primary configuration if it hasn't been set
      if (mPrimaryConfiguration == null) {
          mPrimaryConfiguration = mAllConfigurations[0];
      }        

      if (mAssemblies != null) {
        foreach (ITaskItem item in mAssemblies) {
          try {
            string path = mGac.Resolve(item.ItemSpec, mPrimaryConfiguration, mAllConfigurations);
            if (path != null) {
              string directoryName = Path.GetDirectoryName(path);
              if (!webGACSearchPaths.ContainsKey(directoryName)) {
                webGACSearchPaths.Add(directoryName, null);

                // We also need to check for dependencies
                BuildPathsForAssembly(path, webGACSearchPaths);
              }
            }
          } catch (Exception ex) {
            Log.LogErrorFromException(ex);
          }
        }
      }

      // Add WebGAC search paths to existing search paths.
      foreach (string path in mPaths) {
        if (path.Equals(WebGACSearchPathIndicator, StringComparison.InvariantCultureIgnoreCase)) {
          updatedSearchPaths.AddRange(webGACSearchPaths.Keys);
        } else {
          updatedSearchPaths.Add(path);
        }
      }
      mPaths = updatedSearchPaths.ToArray();

      return true;
    }

    private void BuildPathsForAssembly(string pPath, IDictionary<string, object> pPaths) {
      Log.LogMessage(MessageImportance.Low, "About to load " + pPath);
      Assembly assembly = Assembly.LoadFrom(pPath);
      AssemblyName[] names = assembly.GetReferencedAssemblies();
      foreach (AssemblyName name in names) {
        string referencedPath = mGac.Resolve(name.Name, name.Version, null, null, mPrimaryConfiguration, mAllConfigurations);    // TODO: Add other fields if needed later
        if (referencedPath != null) {
          string directoryName = Path.GetDirectoryName(referencedPath);

          if (!pPaths.ContainsKey(directoryName)) {
            pPaths.Add(directoryName, null);

            BuildPathsForAssembly(referencedPath, pPaths);
          }
        }
      }
    }
  }
}