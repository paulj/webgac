using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WebGACForVS {
  public partial class EditReference : Form {
    private readonly WebGAC.Core.WebGAC mGac;
    private readonly string mAssembly;
    private readonly bool mAllowLocal;
    private Version mSelectedVersion = null;

    public EditReference(WebGAC.Core.WebGAC pGac, string pAssembly, bool pAllowLocal) {
      InitializeComponent();

      mGac = pGac;
      mAssembly = pAssembly;
      mAllowLocal = pAllowLocal;
    }

    public Version SelectedVersion {
      get { return mSelectedVersion; }
    }

    private void EditReference_Load(object sender, EventArgs e) {
      label1.IncreaseBusyCount();

      // Queue up an operation to load the versions
      ThreadPool.QueueUserWorkItem(Thread_LoadVersions);
    }

    private void Thread_LoadVersions(object state) {
      try {
        Version[] versions = mGac.GetAllVersions(mAssembly, mAllowLocal);

        Invoke((ThreadStart) delegate {
                               foreach (Version v in versions) {
                                 versionsListBox.Items.Add(v);
                               }
                             });
      } catch (Exception ex) {
        Invoke((ThreadStart) delegate {
                               MessageBox.Show(this, "Failed to load versions: " + ex.Message, "Failed to load versions",
                                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                             });
      } finally {
        label1.DecreaseBusyCount();
      }
    }

    private void versionsListBox_DoubleClick(object sender, EventArgs e) {
      mSelectedVersion = (Version) versionsListBox.SelectedItem;
      
      DialogResult = System.Windows.Forms.DialogResult.OK;
      Close();
    }

    private void versionsListBox_SelectedValueChanged(object sender, EventArgs e) {
      mSelectedVersion = (Version)versionsListBox.SelectedItem;

      okButton.Enabled = (mSelectedVersion != null);
    }

    private void okButton_Click(object sender, EventArgs e) {
      DialogResult = System.Windows.Forms.DialogResult.OK;
      Close();
    }
  }
}