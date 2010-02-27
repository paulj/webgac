using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using EnvDTE;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Win32;
using VSLangProj80;
using Project=EnvDTE.Project;

namespace WebGACForVS {
  public class BuildExecutor : ILogger {

    public static bool Build(Project pProj, OutputWindowPane pPane, string pTarget) {
      return Build(pProj, pPane, pTarget, new NameValueCollection());
    }

    public static bool Build(Project pProj, OutputWindowPane pPane, string pTarget, NameValueCollection pParams) {
      Microsoft.Build.BuildEngine.Engine buildEngine = new Microsoft.Build.BuildEngine.Engine();
      BuildExecutor executor = new BuildExecutor(pPane);

      RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework", false);
      if (key == null) {
        throw new Exception("Failed to determine .NET Framework install root - no .NETFramework key");
      }
      string installRoot = key.GetValue("InstallRoot") as string;
      if (installRoot == null) {
        throw new Exception("Failed to determine .NET Framework install root - no InstallRoot value");
      }
      key.Close();

      buildEngine.BinPath = Path.Combine(installRoot, string.Format("v{0}.{1}.{2}", Environment.Version.Major, Environment.Version.Minor, Environment.Version.Build));
      buildEngine.RegisterLogger(executor);

      executor.Verbosity = LoggerVerbosity.Normal;

      BuildPropertyGroup properties = new BuildPropertyGroup();
      foreach (string propKey in pParams.Keys) {
        string val = pParams[propKey];

        properties.SetProperty(propKey, val, true);
      }

      return buildEngine.BuildProjectFile(pProj.FileName, new string[]{pTarget}, properties);
    }

    private readonly OutputWindowPane mPane;
    private string mParameters;
    private LoggerVerbosity mVerbosity;

    public BuildExecutor(OutputWindowPane pPane) {
      mPane = pPane;
    }

    public void Initialize(IEventSource eventSource) {
      eventSource.AnyEventRaised += EventSource_HandleAnyEvent;
    }

    public void Shutdown() {
    }

    public LoggerVerbosity Verbosity {
      get { return mVerbosity; }
      set { mVerbosity = value; }
    }

    #region ILogger Members

    public string Parameters {
      get { return mParameters; }
      set { mParameters = value; }
    }

    #endregion
    
    private void EventSource_HandleAnyEvent(object sender, BuildEventArgs e) {
      mPane.OutputString(e.Message + "\n");
    }
  }
}
