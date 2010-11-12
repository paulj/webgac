using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Forms;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using VSLangProj;
using VSLangProj80;
using WebGAC.Core;
using Thread=System.Threading.Thread;

namespace WebGACForVS
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
        private static readonly int StatusSupported = (int)vsCommandStatus.vsCommandStatusSupported;
        private static readonly int StatusEnabled = (int)vsCommandStatus.vsCommandStatusEnabled;
        private static readonly int StyleText = (int)vsCommandStyle.vsCommandStyleText;


        private static void AddCommand(Commands2 c, AddIn a, object[] o, string name, string display, string tip)
        {
            c.AddNamedCommand2(a, name, display, tip, true, 59, ref o, StatusSupported + StatusEnabled, StyleText, vsCommandControlType.vsCommandControlTypeButton);
        }

        private const string Root = "WebGACForVS.Connect.";

        private static Dictionary<string, string> commandNameToLabel = new Dictionary<string, string>
	                                               {
	                                                   {"WebGACForVS.Connect.WebGACConfig", "Configure WebGAC..."},
	                                                   {"WebGACForVS.Connect.WebGACBrowse", "Browse WebGAC..."},
	                                                   {"WebGACForVS.Connect.AddWebGACReference", "Add WebGAC Reference..."},
	                                                   {"WebGACForVS.Connect.UpdateWebGACReferences", "Update WebGAC References..."},
	                                                   {"WebGACForVS.Connect.UpdateWebGACReference", "Update WebGAC Reference..."}
	                                               };

	    private Dictionary<string, Action<Commands2,AddIn,object[]>> contextMenuItems = new Dictionary<string, Action<Commands2,AddIn,object[]>>
	    {
	        {"WebGACForVS.Connect.AddWebGACReference",
	        (Commands2 c, AddIn a, object[] o) => AddCommand(c,a,o,"AddWebGACReference", commandNameToLabel["WebGACForVS.Connect.AddWebGACReference"],
                                                 "Adds a WebGAC Reference to this project")                
	        },
            {"WebGACForVS.Connect.UpdateWebGACReferences",
	        (Commands2 c, AddIn a, object[] o) => AddCommand(c,a,o,"UpdateWebGACReferences", commandNameToLabel["WebGACForVS.Connect.UpdateWebGACReferences"],
                                                 "Updates all WebGAC References to later versions")
	        },
            {"WebGACForVS.Connect.UpdateWebGACReference",
	        (Commands2 c, AddIn a, object[] o) => AddCommand(c,a,o,"UpdateWebGACReference", commandNameToLabel["WebGACForVS.Connect.UpdateWebGACReference"],
                                                 "Updates a WebGAC Reference to a later version")
	        }
	    };

        

        private Dictionary<string, Action<Commands2, AddIn, object[]>> toolMenuItems = new Dictionary<string, Action<Commands2, AddIn, object[]>>
	    {	        
            {"WebGACForVS.Connect.WebGACConfig",
	        (Commands2 c, AddIn a, object[] o) => AddCommand(c,a,o,"WebGACConfig", commandNameToLabel["WebGACForVS.Connect.WebGACConfig"],
                                                 "Configures the WebGAC tools")
	        },
            {"WebGACForVS.Connect.WebGACBrowse",
	        (Commands2 c, AddIn a, object[] o) => AddCommand(c,a,o,"WebGACBrowse", commandNameToLabel["WebGACForVS.Connect.WebGACBrowse"],
                                                 "Browses the WebGAC repositories")
	        }
	    };

	    

	    /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
	    /// <seealso class='IDTExtensibility2' />
	    public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
	    {
	        _applicationObject = (DTE2) application;
	        _addInInstance = (AddIn) addInInst;
	        object[] contextGUIDS = new object[] {};
	        mWndHandleWrapper = new WindowHandleWrapper(_applicationObject);

	        try
	        {
	            Commands2 commands = (Commands2) _applicationObject.Commands;
                Func<string, Command> resolve = (string x) => commands.Item(x, 0);

                // Add all of the commands. Warning: This is only called the first time the add-in is loaded. Later loads of VS do
                // not run this step!

	            foreach (KeyValuePair<string, Action<Commands2, AddIn, object[]>> item in contextMenuItems)
	            {
	                try
	                {
	                    resolve(item.Key);
	                }
	                catch (ArgumentException)
	                {
	                    // Oops, that item didn't exist, so create it and then
	                    // loop back through everything that requires initializing
	                    item.Value.Invoke(commands, _addInInstance, contextGUIDS);
	                }
	            }

	            // Enumerate the command bars, and add our add-ins wherever other relevant options are
	            foreach (CommandBar cmdBar in (CommandBars) _applicationObject.CommandBars)
	            {
	                if (cmdBar.Name == "Reference Root")
	                {	                    
	                    var refRootCmds = new Dictionary<string, int>
	                                          {
	                                              {"AddWebGACReference",2},
	                                              {"UpdateWebGACReferences",cmdBar.Controls.Count}
	                                          };
	                    var keys = new List<string>(refRootCmds.Keys);
                        keys.ForEach(x => MaybeAddCommand(resolve, cmdBar, Root + x, refRootCmds[x]));
	                }
                    if (cmdBar.Name == "Reference Item")
                    {
                        MaybeAddCommand(resolve, cmdBar, Root + "UpdateWebGACReference",1);
                    }
	            }

	            // Place the command on the tools menu.
	            // Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
	            CommandBar menuBarCommandBar = ((CommandBars) _applicationObject.CommandBars)["MenuBar"];

	            // Find the Tools command bar on the MenuBar command bar:
	            string toolsMenuName = FindToolsMenuName();
	            CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
	            CommandBarPopup toolsPopup = (CommandBarPopup) toolsControl;

	            // And then add a control for the command to the tools menu:
                foreach (KeyValuePair<string, Action<Commands2, AddIn, object[]>> item in toolMenuItems)
                {
                    Command com = null;
                    var primed = false;
                    try
                    {
                        com = resolve(item.Key);
                    }
                    catch (ArgumentException)
                    {
                        // Oops, that item didn't exist, so create it and then
                        // loop back through everything that requires initializing
                        item.Value.Invoke(commands, _addInInstance, contextGUIDS);
                        com = resolve(item.Key);
                    }
                    
                    // This is a real hack for the seemingly non-deterministic startup state
                    // Basically have a peek at the elements of the Tool Bar to see if they
                    // already contain the WebGAC elements
                    var toolsControls = toolsPopup.Controls;
                    var it = toolsControls.GetEnumerator();
                    while (it.MoveNext())
                    {                        
                        var commandBarButton = it.Current as CommandBarButton;
                        if (null != commandBarButton)                        
                        {
                            var name = commandBarButton.get_accName(0);
                            if (name != null && name.Contains(commandNameToLabel[item.Key]))
                            {
                                primed = true;
                                break;
                            }                            
                        }
                    }
                    if (!primed)
                    {
                        com.AddControl(toolsPopup.CommandBar, toolsPopup.Controls.Count + 1);
                    }                     
                }	            
	        }
	        catch (Exception ex)
	        {
	            MessageBox.Show(mWndHandleWrapper, ex + "\n" + ex.StackTrace, "Error",
	                            MessageBoxButtons.OK,
	                            MessageBoxIcon.Error);
	        }
	    }

	    private void MaybeAddCommand(Func<string, Command> resolve, CommandBar cmdBar, string candidate, int pos)
	    {
	        var primed = false;

	        foreach (CommandBarControl control in cmdBar.Controls)
	        {
	            var name = control.get_accName(0); 
	            if (null != name)
	            {
	                if (name.Contains(commandNameToLabel[candidate]))
	                {
	                    primed = true;
	                    break;
	                }	                            
	            }
	        }
	        if (!primed)
	        {
                resolve(candidate).AddControl(cmdBar,pos);
	        }
	    }

	    /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
        if (commandName == "WebGACForVS.Connect.AddWebGACReference" || commandName == "WebGACForVS.Connect.UpdateWebGACReferences" || 
            commandName == "WebGACForVS.Connect.UpdateWebGACReference" || commandName == "WebGACForVS.Connect.WebGACConfig" ||
            commandName == "WebGACForVS.Connect.WebGACBrowse" || commandName == "WebGACForVS.Connect.StoreInLocalWebGAC" || 
            commandName.StartsWith("WebGACForVS.Connect.ReleaseToWebGAC"))
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
			if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault) {
        switch (commandName) {
          case "WebGACForVS.Connect.WebGACConfig":
            ConfigureWebGAC configDialog = new ConfigureWebGAC(WebGAC);
            configDialog.ShowDialog(mWndHandleWrapper);

            handled = true;
            return;
          case "WebGACForVS.Connect.WebGACBrowse":
            AddWebGACReference browseDialog = new AddWebGACReference(WebGAC, null, ActiveConfigurationName, AllConfigurationNames);
            browseDialog.ShowDialog(mWndHandleWrapper);

            handled = true;
            return;
        }

        if (_applicationObject.SelectedItems.Count == 0) {
          return;
        }

        SelectedItem item = _applicationObject.SelectedItems.Item(1);
        UIHierarchy UIH = _applicationObject.ToolWindows.SolutionExplorer;
        UIHierarchyItem hItem = (UIHierarchyItem)((System.Array)UIH.SelectedItems).GetValue(0);
        
			  switch (commandName) {

          case "WebGACForVS.Connect.AddWebGACReference":
            Project addProj = FindProject(item, hItem);
            if (addProj != null) {
              VSProject vsProj = (VSProject) addProj.Object;
              AddWebGACReference dialog = new AddWebGACReference(WebGAC, vsProj, ActiveConfigurationName, AllConfigurationNames);
              dialog.ShowDialog(mWndHandleWrapper);
            }
					  handled = true;
            break;

          case "WebGACForVS.Connect.UpdateWebGACReference":
			    Reference reference = null;			   
			    if (hItem.Object is Reference3)
			    {
			        reference = hItem.Object as Reference3;
			    }
                // This case is for F# projects which seem to be automated with a completely different API than C# projects
			    else if (hItem.Object is ProjectItem)
			    {
                    var refItem = hItem.Object as ProjectItem;
			        reference = refItem.Object as Reference;
			    }			    
			    if (reference != null)
			    {
			        // We now need to handle the reference.
			        var dialog = new UpdateWebGACReferences(_applicationObject, WebGAC,
			                                                ActiveConfigurationName,
			                                                AllConfigurationNames, reference);
			        dialog.ShowDialog(mWndHandleWrapper);
			    }
                else
                {
                    MessageBox.Show(
                        "At the moment you can't update an assembly this way in this type of project, please contact your tool vendor.");
                }

			    handled = true;
			    break;

          case "WebGACForVS.Connect.UpdateWebGACReferences":
            Project updateProj = FindProject(item, hItem);
            if (updateProj != null) {
              VSProject vsProj = (VSProject) updateProj.Object;
              List<Reference> references = new List<Reference>();
              foreach (Reference projReference in vsProj.References) {
                references.Add(projReference);
              }
              Reference[] allProjReferences = references.ToArray();

              // We now need to handle the reference.
              UpdateWebGACReferences dialog = new UpdateWebGACReferences(_applicationObject, WebGAC, ActiveConfigurationName, AllConfigurationNames, allProjReferences);
              dialog.ShowDialog(mWndHandleWrapper);
            }
            handled = true;
            break;

          case "WebGACForVS.Connect.StoreInLocalWebGAC":
            Project storeLocalProj = FindProject(item, hItem);
            if (storeLocalProj != null) {
              ExecuteBuildOperation(storeLocalProj, "InstallLocal", new NameValueCollection());
            }
            handled = true;
            break;

          default:
            if (commandName.StartsWith("WebGACForVS.Connect.ReleaseToWebGAC")) {
              string repoNumStr = commandName.Substring("WebGACForVS.Connect.ReleaseToWebGACRepo".Length);
              int repoNum = int.Parse(repoNumStr);
              Project releaseProj = FindProject(item, hItem);
              if (releaseProj != null) {
                NameValueCollection buildParams = new NameValueCollection();
                buildParams["TargetWebGACRepository"] = WebGAC.Config.AllRepositories[repoNum].Url;

                ExecuteBuildOperation(releaseProj, new string[] { "DeployRemote" }, buildParams);
              }
            }
            break;            
				}
			}
		}

    private void ExecuteBuildOperation(Project pProj, string pOperationName, NameValueCollection pBuildParams) {
      ExecuteBuildOperation(pProj, new string[]{pOperationName}, pBuildParams);
    }

	private void ExecuteBuildOperation(Project pProj, string[] pOperationNames, NameValueCollection pBuildParams) {
      try {
        ExecuteTaskInOutputWindow(
          delegate(OutputWindowPane pPane) {
            try {
              NameValueCollection buildParams = new NameValueCollection(pBuildParams);
              
              foreach (string operationName in pOperationNames) {
                if (!BuildExecutor.Build(pProj, pPane, operationName, buildParams)) {
                  MessageBox.Show(mWndHandleWrapper,
                                  "The " + operationName +
                                  " operation failed. Please try attempting a Build first to receive detailed error information.",
                                  operationName + " Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  break;
                }
              }
            } catch (Exception ex) {
              MessageBox.Show(mWndHandleWrapper, "Failed to build project: " + ex.Message);
            }
          });
      } catch (Exception ex) {
        MessageBox.Show(mWndHandleWrapper, "Failed to perform build operation: " + ex.Message);
      }
    }

	  public string ActiveConfigurationName {
        get
        {
            // If there is no project loaded, this returns null
            if (_applicationObject.Solution.SolutionBuild.ActiveConfiguration == null) {
                return null;
            }

            return _applicationObject.Solution.SolutionBuild.ActiveConfiguration.Name;
        }
	  }

	  public string[] AllConfigurationNames {
	    get {
	      List<string> result = new List<string>();
        for (int i = 1; i <= _applicationObject.Solution.SolutionBuild.SolutionConfigurations.Count; ++i) {
              result.Add(_applicationObject.Solution.SolutionBuild.SolutionConfigurations.Item(i).Name); 
        }

	      return result.ToArray();
	    }
	  }

	  private delegate void OutputWindowDelegate(OutputWindowPane pPane);

    private void ExecuteTaskInOutputWindow(OutputWindowDelegate pTask) {
      // Retrieve and show the Output window.
      OutputWindow outWin = _applicationObject.ToolWindows.OutputWindow;
      bool previousAutoHide = outWin.Parent.AutoHides;
      outWin.Parent.AutoHides = false;

      try {
        outWin.Parent.Activate();

        // Find the "Pane1" Output window pane; if it does not exist, 
        // create it.
        OutputWindowPane pane1;
        try {
          pane1 = outWin.OutputWindowPanes.Item("WebGAC");
        } catch {
          pane1 = outWin.OutputWindowPanes.Add("WebGAC");
        }
        pane1.Activate();

        System.Threading.Thread executeThread = new Thread((ThreadStart)
          delegate {
            try {
              pTask(pane1);
            } finally {
              outWin.Parent.AutoHides = previousAutoHide;
            }
          });
        executeThread.SetApartmentState(ApartmentState.STA);
        executeThread.Start();
      } catch {
        outWin.Parent.AutoHides = previousAutoHide;

        throw;
      }
    }

	  public WebGAC.Core.WebGAC WebGAC {
	    get {
	      if (_gac == null) {
	        CredentialsHelper credHelper = new CredentialsHelper();

	        _gac = new WebGAC.Core.WebGAC();
	        _gac.CredRequestHandler = credHelper.HandleCredentialsRequest;
	      }

	      return _gac;
	    }
	  }

    private string FindToolsMenuName() {
		  try {
        //If you would like to move the command to a different menu, change the word "Tools" to the 
        //  English version of the menu. This code will take the culture, append on the name of the menu
        //  then add the command to that menu. You can find a list of all the top-level menus in the file
        //  CommandBar.resx.
        ResourceManager resourceManager =
          new ResourceManager("VSDependencyDownloaderSupport.CommandBar", Assembly.GetExecutingAssembly());
        CultureInfo cultureInfo = new System.Globalization.CultureInfo(_applicationObject.LocaleID);
        string resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
        return resourceManager.GetString(resourceName);
      } catch {
        //We tried to find a localized version of the word Tools, but one was not found.
        //  Default to the en-US word, which may work for the current culture.
        return "Tools";
      }
		}

		private DTE2 _applicationObject;
		private AddIn _addInInstance;
	  private WindowHandleWrapper mWndHandleWrapper;
	  private static WebGAC.Core.WebGAC _gac;   // Make this static so events persist	    
	    private Project FindProject(SelectedItem pItem, UIHierarchyItem pHItem) {
      if (pItem.Project != null) {
        return pItem.Project;
      }

      if (pHItem.Object is Project) {
        return (Project)pHItem.Object;
      } else if (pHItem.Object is ProjectItem) {
        if (((ProjectItem)pHItem.Object).SubProject != null) {
          return ((ProjectItem)pHItem.Object).SubProject;
        } else {
            return ((ProjectItem)pHItem.Object).ContainingProject;
        }
      } else if (pHItem.Collection.Parent is UIHierarchyItem) {
        return FindProject(pItem, (UIHierarchyItem) pHItem.Collection.Parent);
      }

      // TODO: They've selected the "References" folder. So we need another mechanism to work out what project we're in.

      MessageBox.Show(mWndHandleWrapper, "Cannot find Project for selected item!");
      return null;
    }

    private static Reference3 FindReference(UIHierarchyItem pHItem) {
      return pHItem.Object as Reference3;
    }
  }

  internal class WindowHandleWrapper : IWin32Window {
    private readonly DTE2 mAppObject;

    public WindowHandleWrapper(DTE2 pAppObject) {
      mAppObject = pAppObject;
    }

    #region IWin32Window Members
    public IntPtr Handle {
      get { return new IntPtr(mAppObject.MainWindow.HWnd); }
    }
    #endregion
  }
}