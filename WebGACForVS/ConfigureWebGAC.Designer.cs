namespace WebGACForVS {
  partial class ConfigureWebGAC {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.localStoreTextBox = new System.Windows.Forms.TextBox();
      this.localStoreLabel = new System.Windows.Forms.Label();
      this.browseLocalStoreButton = new System.Windows.Forms.Button();
      this.remoteRepositoriesLabel = new System.Windows.Forms.Label();
      this.repositoriesListBox = new System.Windows.Forms.ListBox();
      this.addButton = new System.Windows.Forms.Button();
      this.removeButton = new System.Windows.Forms.Button();
      this.moveUpButton = new System.Windows.Forms.Button();
      this.moveDownButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.browseLocalStoreDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.SuspendLayout();
      // 
      // localStoreTextBox
      // 
      this.localStoreTextBox.Location = new System.Drawing.Point(126, 22);
      this.localStoreTextBox.Name = "localStoreTextBox";
      this.localStoreTextBox.Size = new System.Drawing.Size(396, 20);
      this.localStoreTextBox.TabIndex = 0;
      // 
      // localStoreLabel
      // 
      this.localStoreLabel.AutoSize = true;
      this.localStoreLabel.Location = new System.Drawing.Point(12, 25);
      this.localStoreLabel.Name = "localStoreLabel";
      this.localStoreLabel.Size = new System.Drawing.Size(108, 13);
      this.localStoreLabel.TabIndex = 1;
      this.localStoreLabel.Text = "Local Store Location:";
      // 
      // browseLocalStoreButton
      // 
      this.browseLocalStoreButton.Location = new System.Drawing.Point(533, 19);
      this.browseLocalStoreButton.Name = "browseLocalStoreButton";
      this.browseLocalStoreButton.Size = new System.Drawing.Size(75, 23);
      this.browseLocalStoreButton.TabIndex = 2;
      this.browseLocalStoreButton.Text = "Browse...";
      this.browseLocalStoreButton.UseVisualStyleBackColor = true;
      this.browseLocalStoreButton.Click += new System.EventHandler(this.browseLocalStoreButton_Click);
      // 
      // remoteRepositoriesLabel
      // 
      this.remoteRepositoriesLabel.AutoSize = true;
      this.remoteRepositoriesLabel.Location = new System.Drawing.Point(12, 71);
      this.remoteRepositoriesLabel.Name = "remoteRepositoriesLabel";
      this.remoteRepositoriesLabel.Size = new System.Drawing.Size(108, 13);
      this.remoteRepositoriesLabel.TabIndex = 3;
      this.remoteRepositoriesLabel.Text = "Remote Repositories:";
      // 
      // repositoriesListBox
      // 
      this.repositoriesListBox.FormattingEnabled = true;
      this.repositoriesListBox.Location = new System.Drawing.Point(126, 71);
      this.repositoriesListBox.Name = "repositoriesListBox";
      this.repositoriesListBox.Size = new System.Drawing.Size(396, 160);
      this.repositoriesListBox.TabIndex = 4;
      this.repositoriesListBox.SelectedIndexChanged += new System.EventHandler(this.repositoriesListBox_SelectedIndexChanged);
      // 
      // addButton
      // 
      this.addButton.Location = new System.Drawing.Point(533, 71);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(75, 23);
      this.addButton.TabIndex = 5;
      this.addButton.Text = "Add";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // removeButton
      // 
      this.removeButton.Location = new System.Drawing.Point(533, 100);
      this.removeButton.Name = "removeButton";
      this.removeButton.Size = new System.Drawing.Size(75, 23);
      this.removeButton.TabIndex = 6;
      this.removeButton.Text = "Remove";
      this.removeButton.UseVisualStyleBackColor = true;
      this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
      // 
      // moveUpButton
      // 
      this.moveUpButton.Location = new System.Drawing.Point(533, 142);
      this.moveUpButton.Name = "moveUpButton";
      this.moveUpButton.Size = new System.Drawing.Size(75, 23);
      this.moveUpButton.TabIndex = 7;
      this.moveUpButton.Text = "Move Up";
      this.moveUpButton.UseVisualStyleBackColor = true;
      this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
      // 
      // moveDownButton
      // 
      this.moveDownButton.Location = new System.Drawing.Point(533, 172);
      this.moveDownButton.Name = "moveDownButton";
      this.moveDownButton.Size = new System.Drawing.Size(75, 23);
      this.moveDownButton.TabIndex = 8;
      this.moveDownButton.Text = "Move Down";
      this.moveDownButton.UseVisualStyleBackColor = true;
      this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(447, 259);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 9;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(533, 259);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 10;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // ConfigureWebGAC
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(620, 294);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.moveDownButton);
      this.Controls.Add(this.moveUpButton);
      this.Controls.Add(this.removeButton);
      this.Controls.Add(this.addButton);
      this.Controls.Add(this.repositoriesListBox);
      this.Controls.Add(this.remoteRepositoriesLabel);
      this.Controls.Add(this.browseLocalStoreButton);
      this.Controls.Add(this.localStoreLabel);
      this.Controls.Add(this.localStoreTextBox);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ConfigureWebGAC";
      this.ShowIcon = false;
      this.Text = "Configure WebGAC";
      this.Load += new System.EventHandler(this.ConfigureWebGAC_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox localStoreTextBox;
    private System.Windows.Forms.Label localStoreLabel;
    private System.Windows.Forms.Button browseLocalStoreButton;
    private System.Windows.Forms.Label remoteRepositoriesLabel;
    private System.Windows.Forms.ListBox repositoriesListBox;
    private System.Windows.Forms.Button addButton;
    private System.Windows.Forms.Button removeButton;
    private System.Windows.Forms.Button moveUpButton;
    private System.Windows.Forms.Button moveDownButton;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.FolderBrowserDialog browseLocalStoreDialog;
  }
}