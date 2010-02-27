using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace WebGAC.Core {
  public class CredentialsHelper {
    public NetworkCredential HandleCredentialsRequest(Uri pRequestUri, string pAuthType, NameValueCollection pAuthParams, bool pIsFirst) {
      bool savePwd = true;

      return GetCredentials(pRequestUri.Host, pRequestUri.Port, pAuthType,
                            "Credentials required for " + pAuthParams["realm"] + " at " + pRequestUri.Host,
                            ref savePwd, pRequestUri.Host, !pIsFirst, pAuthParams["realm"]);
    }

    public NetworkCredential HandleReadOnlyCredentialsRequest(Uri pRequestUri, string pAuthType, NameValueCollection pAuthParams, bool pIsFirst) {
      CREDENTIAL cred;
      IntPtr credPtr = IntPtr.Zero;

      try {
        if (CredRead(pAuthParams["realm"] + " on " + pRequestUri.Host, CRED_TYPE.GENERIC, 0, out credPtr)) {
          // Get the Credential from the mem location
          cred = (CREDENTIAL)Marshal.PtrToStructure(credPtr, typeof(CREDENTIAL));

          string username = cred.userName;

          // Get the password
          byte[] passwordBytes = new byte[cred.credentialBlobSize];

          // Copy the memory from the blob to our array
          Marshal.Copy(cred.credentialBlob, passwordBytes, 0, cred.credentialBlobSize);

          // Convert to text
          string passwordText = Encoding.Unicode.GetString(passwordBytes);

          return new NetworkCredential(username, passwordText);
        }
      } finally {
        // Clean up
        if (!credPtr.Equals(IntPtr.Zero)) {
          CredFree(credPtr);
//          Marshal.DestroyStructure(credPtr, typeof (CREDENTIAL));
        }
      }

      return null;
    }

    public static NetworkCredential GetCredentials(string pHost, int pPort, string pAuthType, string pMsg, ref bool savePwd, string pTitle,
                                                   bool pAlwaysShowUI, string pRealm) {
      CREDUI_INFO info = new CREDUI_INFO();
      info.pszCaptionText = pTitle;
      info.pszMessageText = pMsg;

      CREDUI_FLAGS flags =
        //CREDUI_FLAGS.DO_NOT_PERSIST |
        CREDUI_FLAGS.PERSIST |
        CREDUI_FLAGS.GENERIC_CREDENTIALS |
        CREDUI_FLAGS.SHOW_SAVE_CHECK_BOX |
        (pAlwaysShowUI ? CREDUI_FLAGS.ALWAYS_SHOW_UI : 0)/* |
            CREDUI_FLAGS.EXPECT_CONFIRMATION*/
        ;

      string username = "";
      string password = "";

      CredUIReturnCodes result = PromptForCredentials(ref info, pRealm + " on " + pHost, 0, ref username,
                                                      ref password, ref savePwd, flags);

      if (result == CredUIReturnCodes.NO_ERROR) {
        return new NetworkCredential(username, password);
      }

      return null;
    }

    private const int MAX_USER_NAME = 100;
    private const int MAX_PASSWORD = 100;

    private static CredUIReturnCodes PromptForCredentials(
      ref CREDUI_INFO creditUI,
      string targetName,
      int netError,
      ref string userName,
      ref string password,
      ref bool save,
      CREDUI_FLAGS flags) {

      StringBuilder user = new StringBuilder(MAX_USER_NAME);
      StringBuilder pwd = new StringBuilder(MAX_PASSWORD);
      creditUI.cbSize = Marshal.SizeOf(creditUI);

      CredUIReturnCodes result = CredUIPromptForCredentials(
        ref creditUI,
        targetName,
        IntPtr.Zero,
        netError,
        user,
        MAX_USER_NAME,
        pwd,
        MAX_PASSWORD,
        ref save,
        flags);

      userName = user.ToString();
      password = pwd.ToString();

      return result;
    }

    #region Native Methods
    public enum CredUIReturnCodes {
      NO_ERROR = 0,
      ERROR_CANCELLED = 1223,
      ERROR_NO_SUCH_LOGON_SESSION = 1312,
      ERROR_NOT_FOUND = 1168,
      ERROR_INVALID_ACCOUNT_NAME = 1315,
      ERROR_INSUFFICIENT_BUFFER = 122,
      ERROR_INVALID_PARAMETER = 87,
      ERROR_INVALID_FLAGS = 1004,
    }

    public struct CREDUI_INFO {
      public int cbSize;
      public IntPtr hwndParent;
      public string pszMessageText;
      public string pszCaptionText;
      public IntPtr hbmBanner;
    }

    [Flags]
    public enum CREDUI_FLAGS {
      INCORRECT_PASSWORD = 0x1,
      DO_NOT_PERSIST = 0x2,
      REQUEST_ADMINISTRATOR = 0x4,
      EXCLUDE_CERTIFICATES = 0x8,
      REQUIRE_CERTIFICATE = 0x10,
      SHOW_SAVE_CHECK_BOX = 0x40,
      ALWAYS_SHOW_UI = 0x80,
      REQUIRE_SMARTCARD = 0x100,
      PASSWORD_ONLY_OK = 0x200,
      VALIDATE_USERNAME = 0x400,
      COMPLETE_USERNAME = 0x800,
      PERSIST = 0x1000,
      SERVER_CREDENTIAL = 0x4000,
      EXPECT_CONFIRMATION = 0x20000,
      GENERIC_CREDENTIALS = 0x40000,
      USERNAME_TARGET_CREDENTIALS = 0x80000,
      KEEP_USERNAME = 0x100000,
    }

    [DllImport("credui")]
    private static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO creditUR,
                                                                       string targetName,
                                                                       IntPtr reserved1,
                                                                       int iError,
                                                                       StringBuilder userName,
                                                                       int maxUserName,
                                                                       StringBuilder password,
                                                                       int maxPassword,
                                                                       [MarshalAs(UnmanagedType.Bool)] ref bool pfSave,
                                                                       CREDUI_FLAGS flags);

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
    public static extern bool CredFree(IntPtr credentialPtr);

    public enum CRED_TYPE : int {
      GENERIC = 1,
      DOMAIN_PASSWORD = 2,
      DOMAIN_CERTIFICATE = 3,
      DOMAIN_VISIBLE_PASSWORD = 4,
      MAXIMUM = 5
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CREDENTIAL {
      public int flags;
      public int type;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string targetName;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string comment;
      public FILETIME lastWritten;
      public int credentialBlobSize;
      public IntPtr credentialBlob;
      public int persist;
      public int attributeCount;
      public IntPtr credAttribute;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string targetAlias;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string userName;
    } 
    #endregion
  }
}