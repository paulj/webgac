using System;
using System.Collections.Generic;
using System.Text;

namespace WebGAC.Core {
  /// <summary>
  /// Delegate used to receive details of WebGAC errors.
  /// </summary>
  /// <param name="pOperation">the operation being attempted</param>
  /// <param name="pDetails">the details</param>
  public delegate void WebGACErrorHandler(string pOperation, Exception pDetails);
}
