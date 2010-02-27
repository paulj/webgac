namespace WebGACForVS {
  partial class AddWebGACReference {
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
      this.cancelButton = new System.Windows.Forms.Button();
      this.addButton = new System.Windows.Forms.Button();
      this.referencesTreeView = new System.Windows.Forms.TreeView();
      this.loadingLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(344, 229);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 0;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.Location = new System.Drawing.Point(263, 229);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(75, 23);
      this.addButton.TabIndex = 1;
      this.addButton.Text = "Add";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // referencesTreeView
      // 
      this.referencesTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.referencesTreeView.Location = new System.Drawing.Point(12, 12);
      this.referencesTreeView.Name = "referencesTreeView";
      this.referencesTreeView.Size = new System.Drawing.Size(407, 211);
      this.referencesTreeView.TabIndex = 2;
      this.referencesTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.referencesTreeView_BeforeExpand);
      this.referencesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.referencesTreeView_AfterSelect);
      this.referencesTreeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.referencesTreeView_AfterExpand);
      // 
      // loadingLabel
      // 
      this.loadingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.loadingLabel.AutoSize = true;
      this.loadingLabel.Location = new System.Drawing.Point(13, 229);
      this.loadingLabel.Name = "loadingLabel";
      this.loadingLabel.Size = new System.Drawing.Size(54, 13);
      this.loadingLabel.TabIndex = 3;
      this.loadingLabel.Text = "Loading...";
      // 
      // AddWebGACReference
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(431, 264);
      this.Controls.Add(this.loadingLabel);
      this.Controls.Add(this.referencesTreeView);
      this.Controls.Add(this.addButton);
      this.Controls.Add(this.cancelButton);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AddWebGACReference";
      this.ShowIcon = false;
      this.Text = "Add WebGAC Reference";
      this.Load += new System.EventHandler(this.AddWebGACReference_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button addButton;
    private System.Windows.Forms.TreeView referencesTreeView;
    private System.Windows.Forms.Label loadingLabel;
  }
}