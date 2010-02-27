using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WebGACForVS {
  public partial class LoadingControl : Label {
    private int mBusyCount = 0;

    public LoadingControl() {
      InitializeComponent();

      // Default to being invisible
      Visible = false;
    }

    public void IncreaseBusyCount() {
      Invoke((ThreadStart) 
        delegate {
          ++mBusyCount;
          if (mBusyCount == 1) {
            // We've just been activated
            Visible = true;
          }
        });
    }

    public void DecreaseBusyCount() {
      Invoke((ThreadStart)
        delegate {
          if (mBusyCount > 0) {
            --mBusyCount;
            if (mBusyCount == 0) {
              // We've just been de-activated
              Visible = false;
            }
          }
        });
    }
  }
}
