using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WebGAC.Core;

namespace WebGACForVS {
  public partial class ConfigureWebGAC : Form {
    private readonly WebGAC.Core.WebGAC mGac;

    public ConfigureWebGAC(WebGAC.Core.WebGAC pGac) {
      InitializeComponent();

      mGac = pGac;
    }

    private void cancelButton_Click(object sender, EventArgs e) {
      Close();
    }

    private void okButton_Click(object sender, EventArgs e) {
      mGac.Config.LocalStore = localStoreTextBox.Text;

      List<string> newRepos = new List<string>();
      foreach (string repo in repositoriesListBox.Items) {
        newRepos.Add(repo);
      }
      mGac.Config.Repositories = newRepos.ToArray();

      mGac.SaveUserConfig();

      Close();
    }

    private void ConfigureWebGAC_Load(object sender, EventArgs e) {
      this.localStoreTextBox.Text = mGac.Config.LocalStore;

      // Populate the repositories
      foreach (AuthenticatedRepository repository in mGac.Config.AllRepositories) {
        repositoriesListBox.Items.Add(repository.Url);
      }

      // Force an update on the controls
      repositoriesListBox_SelectedIndexChanged(sender, e);
    }

    private void removeButton_Click(object sender, EventArgs e) {
      repositoriesListBox.Items.RemoveAt(repositoriesListBox.SelectedIndex);
    }

    private void repositoriesListBox_SelectedIndexChanged(object sender, EventArgs e) {
      if (repositoriesListBox.SelectedIndex == -1) {
        // Disable the remove, move up, move down buttons
        removeButton.Enabled = false;
        moveUpButton.Enabled = false;
        moveDownButton.Enabled = false;
      } else {
        removeButton.Enabled = true;

        moveUpButton.Enabled = repositoriesListBox.SelectedIndex != 0;
        moveDownButton.Enabled = repositoriesListBox.SelectedIndex < repositoriesListBox.Items.Count - 1;
      }
    }

    private void browseLocalStoreButton_Click(object sender, EventArgs e) {
      browseLocalStoreDialog.SelectedPath = localStoreTextBox.Text;
      if (browseLocalStoreDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
        localStoreTextBox.Text = browseLocalStoreDialog.SelectedPath;
      }
    }

    private void addButton_Click(object sender, EventArgs e) {
      AddRepositoryDialog addDialog = new AddRepositoryDialog(mGac);
      if (addDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
        repositoriesListBox.Items.Add(addDialog.RepositoryUrl);

        // Force an update on the controls
        repositoriesListBox_SelectedIndexChanged(sender, e);
      }
    }

    private void moveUpButton_Click(object sender, EventArgs e) {
      int pos = repositoriesListBox.SelectedIndex;
      object curItem = repositoriesListBox.Items[pos];
      repositoriesListBox.Items.RemoveAt(pos);
      repositoriesListBox.Items.Insert(pos-1, curItem);

      repositoriesListBox.SelectedIndex = pos - 1;
    }

    private void moveDownButton_Click(object sender, EventArgs e) {
      int pos = repositoriesListBox.SelectedIndex;
      object curItem = repositoriesListBox.Items[pos];
      repositoriesListBox.Items.RemoveAt(pos);
      repositoriesListBox.Items.Insert(pos + 1, curItem);

      repositoriesListBox.SelectedIndex = pos + 1;
    }
  }
}