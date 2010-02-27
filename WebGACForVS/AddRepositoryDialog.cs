using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WebGACForVS {
  public partial class AddRepositoryDialog : Form {
    private readonly WebGAC.Core.WebGAC mGac;

    public AddRepositoryDialog(WebGAC.Core.WebGAC pGac) {
      InitializeComponent();

      mGac = pGac;
    }

    public string RepositoryUrl {
      get { return repositoryUrlTextBox.Text; }
      set { repositoryUrlTextBox.Text = value; }
    }

    private void okButton_Click(object sender, EventArgs e) {
      // Test the URL
      try {
        mGac.TestRepositoryUrl(RepositoryUrl);

        this.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.Close();
      } catch (Exception ex) {
        MessageBox.Show(this, "The provided url is invalid. " + ex.Message, "Repository URL is invalid",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }
    }

    private void cancelButton_Click(object sender, EventArgs e) {
      this.Close();
    }
  }
}