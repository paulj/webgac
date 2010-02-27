using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace WebGACForVS
{
  public partial class UploadAssembly : Form
  {
    private WebGAC.Core.WebGAC _wg;

    public UploadAssembly() : this(null)
    {
    }

    public UploadAssembly(WebGAC.Core.WebGAC wg)
    {
      _wg = wg;
      InitializeComponent();
    }

    private void UploadAssembly_Load(object sender, EventArgs e)
    {
      repositoryComboBox.Items.AddRange(_wg.Config.AllRepositories);
      if (repositoryComboBox.Items.Count > 0)
      {
        repositoryComboBox.SelectedIndex = 0;
      }

      buildConfigComboBox.Items.AddRange(new [] { "Release", "Debug" });
      buildConfigComboBox.SelectedIndex = 0;
    }

    private void uploadButton_Click(object sender, EventArgs e)
    {
      try
      {
        Assembly a = Assembly.LoadFile(assemblyFileTextBox.Text);

        _wg.StoreRemote(a.FullName, assemblyFileTextBox.Text, buildConfigComboBox.SelectedItem.ToString(),
                        repositoryComboBox.SelectedItem.ToString());
        DialogResult = DialogResult.OK;
      } 
      catch (Exception ex)
      {
        MessageBox.Show(this, "Failed to upload assembly: " + ex.Message, "Failed to upload assembly",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);  
      }
    }

    private void selectFileButton_Click(object sender, EventArgs e)
    {
      DialogResult res = selectAssemblyFileDialog.ShowDialog(this);
      if (res == DialogResult.OK)
      {
        assemblyFileTextBox.Text = selectAssemblyFileDialog.FileName;
      }
    }

    private void assemblyFileTextBox_TextChanged(object sender, EventArgs e)
    {
      if (!File.Exists(assemblyFileTextBox.Text))
      {
        UpdateUploadDetails(null, null);
        return;
      }
      try
      {
        Assembly a = Assembly.LoadFile(assemblyFileTextBox.Text);
        var details = new WebGAC.Core.AssemblyInfo(a.FullName);
        UpdateUploadDetails(details.Name, details.Version.ToString());
      } 
      catch
      {
        UpdateUploadDetails(null, null);
      }
    }

    private void UpdateUploadDetails(string newAssemblyName, string newVersion)
    {
      if (newAssemblyName == null)
      {
        uploadButton.Enabled = false;
        assemblyName.Text = "<None>";
        assemblyName.ForeColor = System.Drawing.Color.Red;
        assemblyVersion.Text = "<None>";
        assemblyVersion.ForeColor = System.Drawing.Color.Red;
        return;
      }
      uploadButton.Enabled = true;
      assemblyName.Text = newAssemblyName;
      assemblyName.ForeColor = SystemColors.ControlText;
      assemblyVersion.Text = newVersion;
      assemblyVersion.ForeColor = SystemColors.ControlText;
    
    }
  }
}
