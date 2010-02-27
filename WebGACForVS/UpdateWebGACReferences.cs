using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using VSLangProj;
using VSLangProj80;
using Thread=System.Threading.Thread;

namespace WebGACForVS {
  public partial class UpdateWebGACReferences : Form {
    private readonly DTE2 mApplication;
    private readonly WebGAC.Core.WebGAC mGac;
    private readonly string mCurrentConfiguration;
    private readonly string[] mAllConfigurations;
    private readonly Reference3[] mReferences;
    private Thread mUpdateThread;

    public UpdateWebGACReferences(DTE2 pApplication, WebGAC.Core.WebGAC pGac, string pCurrentConfiguration, string[] pAllConfigurations, params Reference3[] pReferences) {
      InitializeComponent();

      mApplication = pApplication;
      mGac = pGac;
      mCurrentConfiguration = pCurrentConfiguration;
      mAllConfigurations = pAllConfigurations;
      mReferences = pReferences;
    }

    private void UpdateWebGACReferences_Load(object sender, EventArgs e) {
      referenceListView.Columns.Add("Component Name");
      referenceListView.Columns.Add("Current Version");
      referenceListView.Columns.Add("Selected Version");
      
      // Add an item for each of our references
      foreach (Reference3 reference in mReferences) {
        ListViewItem item = new ListViewItem(reference.Identity);
        item.SubItems.Add(reference.Version);
        item.SubItems.Add("Checking...", Color.Gray, Color.White, item.Font);

        referenceListView.Items.Add(item);
      }

      // Fix the view
      referenceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

      // Disable the ok button by default. (Only enable it when items are being updated).
      okButton.Enabled = false;

      // Background schedule the items to be updated.
      mUpdateThread = new Thread(Thread_UpdateReferences);
      mUpdateThread.Start();
    }

    private void okButton_Click(object sender, EventArgs e) {
      for (int i = 0; i < mReferences.Length; ++i) {
        Version selected = (Version) referenceListView.Items[i].SubItems[2].Tag;
        Version curVersion = new Version(mReferences[i].Version);
        Reference3 reference = mReferences[i];
        string name = reference.Name;
        
        // Work out if we're updating the reference or not.
        if (selected != null && !curVersion.Equals(selected)) {
          List<Reference3> updateReferences = new List<Reference3>();
          updateReferences.Add(reference); // Always update our reference

          // Check if any other projects in the solution use this reference
          VSProject2 vsProj = (VSProject2)mReferences[i].ContainingProject.Object;
          Reference3[] otherReferences = FindReferenceInOtherProjects(reference.Name, vsProj.Project.UniqueName);
          if (otherReferences.Length > 0) {
            StringBuilder message = new StringBuilder();
            message.AppendFormat("The following projects also have references to {0}:\n", name);
            foreach (Reference3 otherRef in otherReferences) {
              message.AppendFormat("  {0}\n", otherRef.ContainingProject.Name);
            }
            message.Append("Do you wish to update the reference in these projects too?");

            DialogResult result =
              MessageBox.Show(this, message.ToString(), "Other Projects using " + name, MessageBoxButtons.YesNoCancel,
                              MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Cancel) {
              return;
            } else if (result == System.Windows.Forms.DialogResult.Yes) {
              updateReferences.AddRange(otherReferences);
            }
          }

          foreach (Reference3 updateRef in updateReferences) {
            VSProject2 owner = (VSProject2)updateRef.ContainingProject.Object;

            updateRef.Remove();
            owner.References.Add(mGac.Resolve(name, selected, null, null, mCurrentConfiguration, mAllConfigurations));
          }
        }
      }

      this.Close();
    }

    private void cancelButton_Click(object sender, EventArgs e) {
      this.Close();
    }

    private void Thread_UpdateReferences() {
      // Work through each reference
      for (int i = 0; i < mReferences.Length; ++i) {
        if (this.IsDisposed) {
          return;
        }

        if (mReferences[i].SourceProject != null) {
          Invoke((ThreadStart) delegate { referenceListView.Items[i].SubItems[2].Text = "Project Dependency"; });
        } else {
          Version[] versions = mGac.GetAllVersions(mReferences[i].Name, true);

          string verStr = versions.Length > 0 ? versions[0].ToString() : "None";
          Invoke((ThreadStart) delegate {
                                 referenceListView.Items[i].SubItems[2].Text = verStr;
                                 referenceListView.Items[i].SubItems[2].Tag = versions.Length > 0 ? versions[0] : null;
                               });
        }
      }

      Invoke((ThreadStart) delegate { UpdateOkButtonState(); });
    }

    private void referenceListView_DoubleClick(object sender, EventArgs e) {
      Reference3 clickedReference = mReferences[referenceListView.SelectedIndices[0]];

      EditReference edit = new EditReference(mGac, clickedReference.Name, true);
      if (edit.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
        // We need to update the reference
        referenceListView.SelectedItems[0].SubItems[2].Text = edit.SelectedVersion.ToString();
        referenceListView.SelectedItems[0].SubItems[2].Tag = edit.SelectedVersion;
      }
    }

    private void UpdateOkButtonState() {
      bool shouldEnable = false;

      for (int i = 0; i < mReferences.Length; ++i) {
        Version selected = (Version)referenceListView.Items[i].SubItems[2].Tag;

        if (selected != null && selected.ToString() != referenceListView.Items[i].SubItems[1].Text) {
          shouldEnable = true;
        }
      }

      okButton.Enabled = shouldEnable;
    }

    private Reference3[] FindReferenceInOtherProjects(string pRefName, string pExcludesProjectName) {
      List<Reference3> result = new List<Reference3>();

      for (int i = 1; i <= mApplication.Solution.Projects.Count; ++i) {
        Project proj = mApplication.Solution.Projects.Item(i);

        if (proj.UniqueName != pExcludesProjectName && proj.Object is VSProject2) {
          VSProject2 vsProj = (VSProject2)proj.Object;
        
          result.AddRange(GetReferences(vsProj, pRefName));
        } else if (proj.Kind.Equals("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")) {
          // We have a directory style object
          result.AddRange(FindReferencesInFolderItem(proj, pRefName, pExcludesProjectName));
        }
      }

      return result.ToArray();
    }

    private Reference3[] FindReferencesInFolderItem(Project pProjItem, string pRefName, string pExcludesProjectName) {
      List<Reference3> result = new List<Reference3>();

      for (int i = 1; i <= pProjItem.ProjectItems.Count; ++i) {
        ProjectItem projItem = pProjItem.ProjectItems.Item(i);

        if (projItem.SubProject != null) {
          Project proj = projItem.SubProject;

          if (proj.UniqueName != pExcludesProjectName && proj.Object is VSProject2) {
            VSProject2 vsProj = (VSProject2)proj.Object;

            result.AddRange(GetReferences(vsProj, pRefName));
          } else if (proj.Kind.Equals("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")) {
            // We have a directory style object
            result.AddRange(FindReferencesInFolderItem(proj, pRefName, pExcludesProjectName));
          }
        }
      }

      return result.ToArray();
    }

    private Reference3[] GetReferences(VSProject2 pVsProj, string pRefName) {
      List<Reference3> result = new List<Reference3>();

      // See if it has our reference
      for (int j = 1; j <= pVsProj.References.Count; ++j) {
        Reference3 reference = (Reference3) pVsProj.References.Item(j);

        if (reference.Name == pRefName) {
          result.Add(reference);
        }
      }

      return result.ToArray();
    }
  }
}