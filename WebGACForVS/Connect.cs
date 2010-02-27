using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
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
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
    public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom) {
		  _applicationObject = (DTE2) application;
		  _addInInstance = (AddIn) addInInst;
		  mWndHandleWrapper = new WindowHandleWrapper(_applicationObject);

		  try {
        Commands2 commands = (Commands2)_applicationObject.Commands;
		    bool force = false;

        if (connectMode == ext_ConnectMode.ext_cm_Startup) {
          // If we're being told to startup, it seems sometimes that we don't have our controls installed. So test for the existance of one,
          // and if we don't have it, then switch our connect mode to UISetup
          try {
            commands.Item("WebGACForVS.Connect.AddWebGACReference", 0);
          } catch (ArgumentException) {
            force = true;
          }
        }

        // Add all of the commands. Warning: This is only called the first time the add-in is loaded. Later loads of VS do
        // not run this step!
		    if (force || connectMode == ext_ConnectMode.ext_cm_UISetup) {
	        object[] contextGUIDS = new object[] {};
		      
          // Create a Command with name SolnExplContextMenuCS and then add it to the "Item" menubar for the SolutionExplorer
	        Command addWebGACReferenceCommand =
	          commands.AddNamedCommand2(_addInInstance, "AddWebGACReference", "Add WebGAC Reference...",
	                                    "Adds a WebGAC Reference to this project", true, 59, ref contextGUIDS,
	                                    (int) vsCommandStatus.vsCommandStatusSupported +
	                                    (int) vsCommandStatus.vsCommandStatusEnabled,
	                                    (int) vsCommandStyle.vsCommandStyleText,
	                                    vsCommandControlType.vsCommandControlTypeButton);
	        Command updateWebGACReferencesCommand =
	          commands.AddNamedCommand2(_addInInstance, "UpdateWebGACReferences", "Update WebGAC References...",
	                                    "Updates all WebGAC References to later versions", true, 59, ref contextGUIDS,
	                                    (int) vsCommandStatus.vsCommandStatusSupported +
	                                    (int) vsCommandStatus.vsCommandStatusEnabled,
	                                    (int) vsCommandStyle.vsCommandStyleText,
	                                    vsCommandControlType.vsCommandControlTypeButton);
	        Command updateWebGACReferenceCommand =
	          commands.AddNamedCommand2(_addInInstance, "UpdateWebGACReference", "Update WebGAC Reference...",
	                                    "Updates a WebGAC Reference to a later version", true, 59, ref contextGUIDS,
	                                    (int) vsCommandStatus.vsCommandStatusSupported +
	                                    (int) vsCommandStatus.vsCommandStatusEnabled,
	                                    (int) vsCommandStyle.vsCommandStyleText,
	                                    vsCommandControlType.vsCommandControlTypeButton);

          Command installLocalCommand =
            commands.AddNamedCommand2(_addInInstance, "StoreInLocalWebGAC", "Install into local WebGAC",
	                                    "Installs the project output into the local WebGAC", true, 59, ref contextGUIDS,
	                                    (int) vsCommandStatus.vsCommandStatusSupported +
	                                    (int) vsCommandStatus.vsCommandStatusEnabled,
	                                    (int) vsCommandStyle.vsCommandStyleText,
	                                    vsCommandControlType.vsCommandControlTypeButton);

          // Enumerate the command bars, and add our add-ins wherever other relevant options are
	        foreach (CommandBar cmdBar in (CommandBars) _applicationObject.CommandBars) {
	          int dependentCodePos = -1;
	          int viewObjectBrowserPos = -1;

	          foreach (CommandBarControl control in cmdBar.Controls) {
              if (control.Caption.Contains("Add &Reference...")) {
	              // TODO: Also install these into the project menu.

	              addWebGACReferenceCommand.AddControl(cmdBar, control.Index + 2); // After Add Web Reference
	              updateWebGACReferencesCommand.AddControl(cmdBar, control.Index + 3);
	            }

	            if (control.Caption.Contains("Find Dependent Code")) {
	              dependentCodePos = control.Index;
              } else if (control.Caption.Contains("&View in &Object Browser")) {
	              viewObjectBrowserPos = control.Index;
	            }

              if (control.Caption.StartsWith("B&uild") && cmdBar.Controls.Count >= control.Index + 4) {
                installLocalCommand.AddControl(cmdBar, control.Index + 4);
                
                /*CommandBarPopup popup = (CommandBarPopup)
                  cmdBar.Controls.Add(MsoControlType.msoControlPopup, Type.Missing, System.Type.Missing, control.Index + 5, true);
                popup.Caption = "Release to WebGAC";*/

                // Request a divider before the install local command
                cmdBar.Controls[control.Index + 4].BeginGroup = true;
              }
	          }

            if (dependentCodePos != -1 && viewObjectBrowserPos != -1) {
              updateWebGACReferenceCommand.AddControl(cmdBar, dependentCodePos + 1);
            }
	        }

	        // Place the command on the tools menu.
	        //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
	        CommandBar menuBarCommandBar = ((CommandBars) _applicationObject.CommandBars)["MenuBar"];

	        //        CommandBar solutionExplorerCommandBar 

	        //Find the Tools command bar on the MenuBar command bar:
	        string toolsMenuName = FindToolsMenuName();
	        CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
	        CommandBarPopup toolsPopup = (CommandBarPopup) toolsControl;

	        //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
	        //  just make sure you also update the QueryStatus/Exec method to include the new command names.
	        //Add a command to the Commands collection:
          Command configureCommand =
            commands.AddNamedCommand2(_addInInstance, "WebGACConfig", "Configure WebGAC...",
                                      "Configures the WebGAC tools", true, 59,
                                      ref contextGUIDS,
                                      (int) vsCommandStatus.vsCommandStatusSupported +
                                      (int) vsCommandStatus.vsCommandStatusEnabled,
                                      (int) vsCommandStyle.vsCommandStyleText,
                                      vsCommandControlType.vsCommandControlTypeButton);
          Command browseCommand =
            commands.AddNamedCommand2(_addInInstance, "WebGACBrowse", "Browse WebGAC...",
                                      "Browses the WebGAC repositories", true, 59,
                                      ref contextGUIDS,
                                      (int)vsCommandStatus.vsCommandStatusSupported +
                                      (int)vsCommandStatus.vsCommandStatusEnabled,
                                      (int)vsCommandStyle.vsCommandStyleText,
                                      vsCommandControlType.vsCommandControlTypeButton);

          // Add a control for the command to the tools menu:
          if ((configureCommand != null) && (toolsPopup != null)) {
            configureCommand.AddControl(toolsPopup.CommandBar, toolsPopup.Controls.Count + 1);
            browseCommand.AddControl(toolsPopup.CommandBar, toolsPopup.Controls.Count + 1);
          }
		    }

        if (connectMode == ext_ConnectMode.ext_cm_Startup) {
          // Create the list of commands for repo updates
          UpdateRepositoryReleaseCommands(commands);
          WebGAC.Config.RepositoriesUpdated += delegate { UpdateRepositoryReleaseCommands(commands); };

          // Add a listener on the repository list, and also update the repositories popup
          foreach (CommandBar cmdBar in (CommandBars)_applicationObject.CommandBars) {
            foreach (CommandBarControl control in cmdBar.Controls) {
              if (control.Caption.StartsWith("B&uild") && cmdBar.Controls.Count >= control.Index + 4) {
                // Install the release popup every time
                CommandBarPopup popup = (CommandBarPopup)
                  cmdBar.Controls.Add(MsoControlType.msoControlPopup, Type.Missing, System.Type.Missing, control.Index + 5, true);
                popup.Caption = "Release to WebGAC";

                // Add events for updating the release popup
                WebGAC.Config.RepositoriesUpdated += delegate { UpdateRepositoriesPopup(commands, popup); };
                UpdateRepositoriesPopup(commands, popup);

                // Request a divider before the install local command
                cmdBar.Controls[control.Index + 4].BeginGroup = true;
              }
            }
          }
        }
      } catch (Exception ex) {
        System.Windows.Forms.MessageBox.Show(mWndHandleWrapper, ex.ToString() + "\n" + ex.StackTrace, "Error",
                                               System.Windows.Forms.MessageBoxButtons.OK,
                                               System.Windows.Forms.MessageBoxIcon.Error);
      }
		}

    private void UpdateRepositoryReleaseCommands(Commands2 pCmds) {
      object[] contextGUIDS = new object[] { };

      for (int i = 0; i < WebGAC.Config.AllRepositories.Length; ++i) {
        
        try {
          Command cmd = pCmds.Item("WebGACForVS.Connect.ReleaseToWebGACRepo" + i, 0);
        } catch {
          // We need to add a new one
          pCmds.AddNamedCommand2(_addInInstance, "ReleaseToWebGACRepo" + i, WebGAC.Config.AllRepositories[i].Url,
                                 "Releases a project into the WebGAC", true, 59, ref contextGUIDS,
                                 (int)vsCommandStatus.vsCommandStatusSupported +
                                 (int)vsCommandStatus.vsCommandStatusEnabled,
                                 (int)vsCommandStyle.vsCommandStyleText,
                                 vsCommandControlType.vsCommandControlTypeButton);
        }
      }

      // Remove the excess controls
      int j = WebGAC.Config.AllRepositories.Length;
      while (true) {
        try {
          Command repoCmd = pCmds.Item("WebGACForVS.Connect.ReleaseToWebGACRepo" + j, 0);
          repoCmd.Delete();

          ++j;
        } catch {
          return;
        }
      }
    }

    private void UpdateRepositoriesPopup(Commands2 pCmds, CommandBarPopup pPopup) {
      object[] contextGUIDS = new object[] { };

      for (int i = 0; i < WebGAC.Config.AllRepositories.Length; ++i) {
        if (pPopup.Controls.Count <= i) {
          Command repoCmd = pCmds.Item("WebGACForVS.Connect.ReleaseToWebGACRepo" + i, 0);
          repoCmd.AddControl(pPopup.CommandBar, i + 1);
        }

        // Make sure the name is correct
        pPopup.Controls[i + 1].Caption = WebGAC.Config.AllRepositories[i].Url;
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
              VSProject2 vsProj = (VSProject2) addProj.Object;
              AddWebGACReference dialog = new AddWebGACReference(WebGAC, vsProj, ActiveConfigurationName, AllConfigurationNames);
              dialog.ShowDialog(mWndHandleWrapper);
            }
					  handled = true;
            break;

          case "WebGACForVS.Connect.UpdateWebGACReference":
			      Reference3 reference = FindReference(hItem);
            if (reference != null) {
              // We now need to handle the reference.
              UpdateWebGACReferences dialog = new UpdateWebGACReferences(_applicationObject, WebGAC, ActiveConfigurationName, AllConfigurationNames, reference);
              dialog.ShowDialog(mWndHandleWrapper);
            }

            handled = true;
            break;

          case "WebGACForVS.Connect.UpdateWebGACReferences":
            Project updateProj = FindProject(item, hItem);
            if (updateProj != null) {
              VSProject2 vsProj = (VSProject2) updateProj.Object;
              List<Reference3> references = new List<Reference3>();
              foreach (Reference3 projReference in vsProj.References) {
                references.Add(projReference);
              }
              Reference3[] allProjReferences = references.ToArray();

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
        return ((ProjectItem)pHItem.Object).ContainingProject;
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