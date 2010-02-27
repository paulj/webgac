namespace WebGACForVS
{
  partial class UploadAssembly
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.repositoryLabel = new System.Windows.Forms.Label();
      this.repositoryComboBox = new System.Windows.Forms.ComboBox();
      this.fileLabel = new System.Windows.Forms.Label();
      this.assemblyFileTextBox = new System.Windows.Forms.TextBox();
      this.selectFileButton = new System.Windows.Forms.Button();
      this.buildConfigLabel = new System.Windows.Forms.Label();
      this.buildConfigComboBox = new System.Windows.Forms.ComboBox();
      this.cancelButton = new System.Windows.Forms.Button();
      this.uploadButton = new System.Windows.Forms.Button();
      this.selectAssemblyFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.uploadDetailsBox = new System.Windows.Forms.GroupBox();
      this.assemblyNameLabel = new System.Windows.Forms.Label();
      this.versionLabel = new System.Windows.Forms.Label();
      this.assemblyName = new System.Windows.Forms.Label();
      this.assemblyVersion = new System.Windows.Forms.Label();
      this.uploadDetailsBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // repositoryLabel
      // 
      this.repositoryLabel.AutoSize = true;
      this.repositoryLabel.Location = new System.Drawing.Point(12, 9);
      this.repositoryLabel.Name = "repositoryLabel";
      this.repositoryLabel.Size = new System.Drawing.Size(60, 13);
      this.repositoryLabel.TabIndex = 0;
      this.repositoryLabel.Text = "Repository:";
      // 
      // repositoryComboBox
      // 
      this.repositoryComboBox.FormattingEnabled = true;
      this.repositoryComboBox.Location = new System.Drawing.Point(93, 9);
      this.repositoryComboBox.Name = "repositoryComboBox";
      this.repositoryComboBox.Size = new System.Drawing.Size(353, 21);
      this.repositoryComboBox.TabIndex = 1;
      // 
      // fileLabel
      // 
      this.fileLabel.AutoSize = true;
      this.fileLabel.Location = new System.Drawing.Point(13, 37);
      this.fileLabel.Name = "fileLabel";
      this.fileLabel.Size = new System.Drawing.Size(26, 13);
      this.fileLabel.TabIndex = 2;
      this.fileLabel.Text = "File:";
      // 
      // assemblyFileTextBox
      // 
      this.assemblyFileTextBox.Location = new System.Drawing.Point(93, 34);
      this.assemblyFileTextBox.Name = "assemblyFileTextBox";
      this.assemblyFileTextBox.Size = new System.Drawing.Size(319, 20);
      this.assemblyFileTextBox.TabIndex = 3;
      this.assemblyFileTextBox.TextChanged += new System.EventHandler(this.assemblyFileTextBox_TextChanged);
      // 
      // selectFileButton
      // 
      this.selectFileButton.Location = new System.Drawing.Point(419, 32);
      this.selectFileButton.Name = "selectFileButton";
      this.selectFileButton.Size = new System.Drawing.Size(27, 23);
      this.selectFileButton.TabIndex = 4;
      this.selectFileButton.Text = "...";
      this.selectFileButton.UseVisualStyleBackColor = true;
      this.selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
      // 
      // buildConfigLabel
      // 
      this.buildConfigLabel.AutoSize = true;
      this.buildConfigLabel.Location = new System.Drawing.Point(13, 62);
      this.buildConfigLabel.Name = "buildConfigLabel";
      this.buildConfigLabel.Size = new System.Drawing.Size(66, 13);
      this.buildConfigLabel.TabIndex = 5;
      this.buildConfigLabel.Text = "Build Config:";
      // 
      // buildConfigComboBox
      // 
      this.buildConfigComboBox.FormattingEnabled = true;
      this.buildConfigComboBox.Location = new System.Drawing.Point(93, 59);
      this.buildConfigComboBox.Name = "buildConfigComboBox";
      this.buildConfigComboBox.Size = new System.Drawing.Size(196, 21);
      this.buildConfigComboBox.TabIndex = 6;
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(371, 177);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 7;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // uploadButton
      // 
      this.uploadButton.Location = new System.Drawing.Point(290, 177);
      this.uploadButton.Name = "uploadButton";
      this.uploadButton.Size = new System.Drawing.Size(75, 23);
      this.uploadButton.TabIndex = 8;
      this.uploadButton.Text = "Upload";
      this.uploadButton.UseVisualStyleBackColor = true;
      this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
      // 
      // selectAssemblyFileDialog
      // 
      this.selectAssemblyFileDialog.DefaultExt = "dll";
      this.selectAssemblyFileDialog.Filter = "Assembly files|*.dll|All files|*.*";
      // 
      // uploadDetailsBox
      // 
      this.uploadDetailsBox.Controls.Add(this.assemblyVersion);
      this.uploadDetailsBox.Controls.Add(this.assemblyName);
      this.uploadDetailsBox.Controls.Add(this.versionLabel);
      this.uploadDetailsBox.Controls.Add(this.assemblyNameLabel);
      this.uploadDetailsBox.Location = new System.Drawing.Point(13, 90);
      this.uploadDetailsBox.Name = "uploadDetailsBox";
      this.uploadDetailsBox.Size = new System.Drawing.Size(433, 81);
      this.uploadDetailsBox.TabIndex = 9;
      this.uploadDetailsBox.TabStop = false;
      this.uploadDetailsBox.Text = "Upload Details";
      // 
      // assemblyNameLabel
      // 
      this.assemblyNameLabel.AutoSize = true;
      this.assemblyNameLabel.ForeColor = System.Drawing.SystemColors.ControlText;
      this.assemblyNameLabel.Location = new System.Drawing.Point(6, 26);
      this.assemblyNameLabel.Name = "assemblyNameLabel";
      this.assemblyNameLabel.Size = new System.Drawing.Size(85, 13);
      this.assemblyNameLabel.TabIndex = 0;
      this.assemblyNameLabel.Text = "Assembly Name:";
      // 
      // versionLabel
      // 
      this.versionLabel.AutoSize = true;
      this.versionLabel.Location = new System.Drawing.Point(6, 51);
      this.versionLabel.Name = "versionLabel";
      this.versionLabel.Size = new System.Drawing.Size(45, 13);
      this.versionLabel.TabIndex = 1;
      this.versionLabel.Text = "Version:";
      // 
      // assemblyName
      // 
      this.assemblyName.AutoSize = true;
      this.assemblyName.ForeColor = System.Drawing.Color.Red;
      this.assemblyName.Location = new System.Drawing.Point(97, 26);
      this.assemblyName.Name = "assemblyName";
      this.assemblyName.Size = new System.Drawing.Size(45, 13);
      this.assemblyName.TabIndex = 2;
      this.assemblyName.Text = "<None>";
      // 
      // assemblyVersion
      // 
      this.assemblyVersion.AutoSize = true;
      this.assemblyVersion.ForeColor = System.Drawing.Color.Red;
      this.assemblyVersion.Location = new System.Drawing.Point(97, 51);
      this.assemblyVersion.Name = "assemblyVersion";
      this.assemblyVersion.Size = new System.Drawing.Size(45, 13);
      this.assemblyVersion.TabIndex = 3;
      this.assemblyVersion.Text = "<None>";
      // 
      // UploadAssembly
      // 
      this.AcceptButton = this.uploadButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(458, 212);
      this.Controls.Add(this.uploadDetailsBox);
      this.Controls.Add(this.uploadButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.buildConfigComboBox);
      this.Controls.Add(this.buildConfigLabel);
      this.Controls.Add(this.selectFileButton);
      this.Controls.Add(this.assemblyFileTextBox);
      this.Controls.Add(this.fileLabel);
      this.Controls.Add(this.repositoryComboBox);
      this.Controls.Add(this.repositoryLabel);
      this.Name = "UploadAssembly";
      this.Text = "Upload Assembly";
      this.Load += new System.EventHandler(this.UploadAssembly_Load);
      this.uploadDetailsBox.ResumeLayout(false);
      this.uploadDetailsBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label repositoryLabel;
    private System.Windows.Forms.ComboBox repositoryComboBox;
    private System.Windows.Forms.Label fileLabel;
    private System.Windows.Forms.TextBox assemblyFileTextBox;
    private System.Windows.Forms.Button selectFileButton;
    private System.Windows.Forms.Label buildConfigLabel;
    private System.Windows.Forms.ComboBox buildConfigComboBox;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button uploadButton;
    private System.Windows.Forms.OpenFileDialog selectAssemblyFileDialog;
    private System.Windows.Forms.GroupBox uploadDetailsBox;
    private System.Windows.Forms.Label versionLabel;
    private System.Windows.Forms.Label assemblyNameLabel;
    private System.Windows.Forms.Label assemblyVersion;
    private System.Windows.Forms.Label assemblyName;
  }
}