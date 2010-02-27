namespace WebGACForVS {
  partial class AddRepositoryDialog {
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
      this.repositoryUrlLabel = new System.Windows.Forms.Label();
      this.repositoryUrlTextBox = new System.Windows.Forms.TextBox();
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // repositoryUrlLabel
      // 
      this.repositoryUrlLabel.AutoSize = true;
      this.repositoryUrlLabel.Location = new System.Drawing.Point(12, 9);
      this.repositoryUrlLabel.Name = "repositoryUrlLabel";
      this.repositoryUrlLabel.Size = new System.Drawing.Size(85, 13);
      this.repositoryUrlLabel.TabIndex = 0;
      this.repositoryUrlLabel.Text = "Repository URL:";
      // 
      // repositoryUrlTextBox
      // 
      this.repositoryUrlTextBox.Location = new System.Drawing.Point(103, 6);
      this.repositoryUrlTextBox.Name = "repositoryUrlTextBox";
      this.repositoryUrlTextBox.Size = new System.Drawing.Size(363, 20);
      this.repositoryUrlTextBox.TabIndex = 1;
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(391, 39);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 3;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(309, 39);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 2;
      this.okButton.Text = "Add";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // AddRepositoryDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(478, 74);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.repositoryUrlTextBox);
      this.Controls.Add(this.repositoryUrlLabel);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AddRepositoryDialog";
      this.ShowIcon = false;
      this.Text = "Add Repository";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label repositoryUrlLabel;
    private System.Windows.Forms.TextBox repositoryUrlTextBox;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button okButton;
  }
}