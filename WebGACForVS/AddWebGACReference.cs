using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using VSLangProj80;

namespace WebGACForVS {
  public partial class AddWebGACReference : Form {
    private readonly VSProject2 mVsProj;
    private readonly WebGAC.Core.WebGAC mGac;
    private readonly string mCurrentConfiguration;
    private readonly string[] mAllConfigurations;
    private Thread mLoadThread;

    public AddWebGACReference(WebGAC.Core.WebGAC pGac, VSProject2 pVsProj, string pCurrentConfiguration, string[] pAllConfigurations) {
      InitializeComponent();

      mVsProj = pVsProj;
      mCurrentConfiguration = pCurrentConfiguration;
      mAllConfigurations = pAllConfigurations;
      mGac = pGac;
    }

    private void AddWebGACReference_Load(object sender, EventArgs e) {
      // Allow the pVsProj to be null - turn this into a browse dialog
      if (mVsProj == null) {
        addButton.Enabled = false;
        addButton.Visible = false;

        cancelButton.Text = "Close";

        this.Text = "Browse WebGAC";
      }

      DoLoad();
    }

    private void cancelButton_Click(object sender, EventArgs e) {
      this.Close();
    }

    private void addButton_Click(object sender, EventArgs e) {
      try {
        if (referencesTreeView.SelectedNode != null && referencesTreeView.SelectedNode is VersionNode) {
          VersionNode version = (VersionNode) referencesTreeView.SelectedNode;

          addButton.Enabled = false;
          cancelButton.Enabled = false;
          loadingLabel.Text = "Working...";
          loadingLabel.Visible = true;

          VersionWorkerDelegate worker = Thread_ResolveReference;
          worker.BeginInvoke(version, delegate (IAsyncResult pResult) {
            if (worker.EndInvoke(pResult)) {
              Invoke((ThreadStart)delegate { this.Close(); });
              return;
            }

            Invoke((ThreadStart)delegate {
              addButton.Enabled = true;
              cancelButton.Enabled = true;
              loadingLabel.Visible = false;
            });
          }, null);
          return;
        }

        this.Close();
      } catch (Exception ex) {
        MessageBox.Show(this, "Failed to add reference: " + ex.Message, "Failed to Add Reference", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }
    }

    private void Thread_AddReferences() {
      try {
        string[] assemblies = mGac.GetAllAssemblies();

        if (this.IsDisposed) {
          return;
        }

        Invoke((ThreadStart)delegate {
          foreach (string assembly in assemblies) {
            TreeNode node = new AssemblyNode(assembly);
            referencesTreeView.Nodes.Add(node);

            // Add the temporary "loading" node
            node.Nodes.Add(new LoadingNode());
          }

          loadingLabel.Visible = false;
        });
      } catch (Exception ex) {
        if (this.IsDisposed) {
          return;
        }

        Invoke((ThreadStart)delegate {
          referencesTreeView.Nodes.Clear();

          referencesTreeView.Nodes.Add(new ErrorNode("Failed to retrieve assemblies: " + ex.Message));
        });
      }
    }

    private void referencesTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
    }

    private void referencesTreeView_AfterExpand(object sender, TreeViewEventArgs e) {
      if (e.Node is AssemblyNode && e.Node.FirstNode != null && e.Node.FirstNode is LoadingNode) {
        // We need to load the nodes
        AssemblyNode assembly = (AssemblyNode) e.Node;
        AssemblyWorkerDelegate loadAssembly = Thread_LoadAssemblyVersions;
        loadAssembly.BeginInvoke(assembly, null, null);
      }
    }

    private void DoLoad()
    {
      referencesTreeView.Nodes.Clear();
      loadingLabel.Visible = true;

      mLoadThread = new Thread(Thread_AddReferences);
      mLoadThread.Start();

      addButton.Enabled = false;
    }

    private delegate void AssemblyWorkerDelegate(AssemblyNode pNode);
    private void Thread_LoadAssemblyVersions(AssemblyNode pNode) {
      try {
        Version[] versions = mGac.GetAllVersions(pNode.AssemblyName, true);

        Invoke((ThreadStart) delegate {
          pNode.Nodes.Clear();

          if (versions.Length == 0) {
            pNode.Nodes.Add(new ErrorNode("No Versions Available"));
          } else {
            foreach (Version version in versions) {
              pNode.Nodes.Add(new VersionNode(pNode.AssemblyName, version));
            }
          }
        });
      } catch (Exception ex) {
        Invoke((ThreadStart)delegate {
          pNode.Nodes.Clear();

          pNode.Nodes.Add(new ErrorNode("Failed to retrieve versions: " + ex.Message));
        });
      }
    }

    private delegate bool VersionWorkerDelegate(VersionNode pNode);
    private bool Thread_ResolveReference(VersionNode pNode) {

      string refPath = mGac.Resolve(pNode.AssemblyName, pNode.Version, null, null, mCurrentConfiguration, mAllConfigurations);
      if (refPath == null) {
        Invoke((ThreadStart) delegate {
                               MessageBox.Show(this,
                                               "Failed to add reference: Assembly was not available in any repository",
                                               "Failed to Add Reference", MessageBoxButtons.OK,
                                               MessageBoxIcon.Error);
                             });
        return false;
      }

      try {
        Reference3 reference = (Reference3) mVsProj.References.Add(refPath);
        reference.SpecificVersion = true;
        return true;
      } catch (Exception ex) {
        Invoke((ThreadStart) delegate {
                               MessageBox.Show(this, "Failed to add reference: " + ex.Message, "Failed to Add Reference",
                                               MessageBoxButtons.OK,
                                               MessageBoxIcon.Error);
                             });
        return false;
      }
    }

    private void referencesTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
      if (addButton.Visible) {
        addButton.Enabled = (referencesTreeView.SelectedNode is VersionNode);
      }
    }

    private void uploadButton_Click(object sender, EventArgs e)
    {
      UploadAssembly ua = new UploadAssembly(mGac);
      if (ua.ShowDialog(this) == DialogResult.OK)
      {
        DoLoad();
      }
    }
  }

  /// <summary>
  /// Node used to represent an assembly in the tree.
  /// </summary>
  internal class AssemblyNode : TreeNode {
    public AssemblyNode(string pAssemblyName) {
      Text = pAssemblyName;
    }

    public string AssemblyName {
      get { return Text; }
    }
  }

  /// <summary>
  /// Node used as a placeholder for items that have not yet had their contents loaded.
  /// </summary>
  internal class LoadingNode : TreeNode {
    public LoadingNode() {
      Text = "Loading...";
    }
  }

  internal class ErrorNode : TreeNode {
    public ErrorNode(string pError) {
      Text = pError;

      this.ForeColor = Color.Red;
    }
  }

  internal class VersionNode : TreeNode {
    private readonly string mAssemblyName;

    public VersionNode(string pAssemblyName, Version pVersion) {
      mAssemblyName = pAssemblyName;

      Text = pVersion.ToString();
    }

    public string AssemblyName {
      get { return mAssemblyName; }
    }

    public Version Version {
      get { return new Version(Text); }
    }
  }
}