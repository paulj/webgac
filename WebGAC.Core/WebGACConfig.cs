using System.Collections.Generic;
using System.Xml.Serialization;

namespace WebGAC.Core {
  /// <summary>
  /// Object that is serialized to/from XML for configuring the WebGAC.
  /// </summary>
  public class WebGACConfig {
    private string mLocalStore;
    private string[] mRepositories;
    private AuthenticatedRepository[] mAuthenticatedRepositories;

    /// <summary>
    /// The local store for downloaded files.
    /// </summary>
    public string LocalStore {
      get { return mLocalStore; }
      set {
        mLocalStore = value;

        WebGACConfigChangeHandler updated = LocalStoreUpdated;
        if (updated != null) {
          updated(this);
        }
      }
    }

    /// <summary>
    /// The remote repositories to download content from.
    /// </summary>
    [XmlArrayItem("Repository")]
    public string[] Repositories {
      get { return mRepositories; }
      set {
        mRepositories = value;

        WebGACConfigChangeHandler updated = RepositoriesUpdated;
        if (updated != null) {
          updated(this);
        }
      }
    }

    [XmlArrayItem("AuthenticatedRepository")]
    public AuthenticatedRepository[] AuthenticatedRepositories {
      get { return mAuthenticatedRepositories; }
      set { mAuthenticatedRepositories = value; }
    }

    [XmlIgnore]
    public AuthenticatedRepository[] AllRepositories {
      get {
        List<AuthenticatedRepository> result = new List<AuthenticatedRepository>();
        result.AddRange(AuthenticatedRepositories);
        foreach (string repos in Repositories) {
          result.Add(new AuthenticatedRepository(repos, null, null));
        }

        return result.ToArray();
      }
    }

    /// <summary>
    /// Merges in the provided configuration.
    /// </summary>
    public void Merge(WebGACConfig pOther) {
      LocalStore = pOther.LocalStore;
      if (Repositories == null || Repositories.Length == 0) {
        Repositories = pOther.Repositories;
      } else {
        List<string> repos = new List<string>();
        repos.AddRange(Repositories);
        repos.AddRange(pOther.Repositories);

        Repositories = repos.ToArray();
      }
      if (AuthenticatedRepositories == null || AuthenticatedRepositories.Length == 0) {
        AuthenticatedRepositories = pOther.AuthenticatedRepositories;
      } else {
        List<AuthenticatedRepository> repos = new List<AuthenticatedRepository>();
        repos.AddRange(AuthenticatedRepositories);
        repos.AddRange(pOther.AuthenticatedRepositories);

        AuthenticatedRepositories = repos.ToArray();
      }
    }

    /// <summary>
    /// Event fired when the list of repositories is updated.
    /// </summary>
    public event WebGACConfigChangeHandler RepositoriesUpdated;

    /// <summary>
    /// Event fired when the local store is updated.
    /// </summary>
    public event WebGACConfigChangeHandler LocalStoreUpdated;
  }

  public class AuthenticatedRepository {
    private string mUrl;
    private string mUsername;
    private string mPassword;

    public AuthenticatedRepository() {
    }

    public AuthenticatedRepository(string pUrl, string pUsername, string pPassword) {
      mUrl = pUrl;
      mUsername = pUsername;
      mPassword = pPassword;
    }

    public string Url {
      get { return mUrl; }
      set { mUrl = value; }
    }

    public string Username {
      get { return mUsername; }
      set { mUsername = value; }
    }

    public string Password {
      get { return mPassword; }
      set { mPassword = value; }
    }

    public override string ToString() {
      if (mUsername == null || mPassword == null) {
        return mUrl;
      }

      return mUsername + ":" + mPassword + "@" + mUrl;
    }

    public override int GetHashCode() {
      return ToString().GetHashCode();
    }
  }

  /// <summary>
  /// Delegate used for informing of changes to the config.
  /// </summary>
  /// <param name="pConfig"></param>
  public delegate void WebGACConfigChangeHandler(WebGACConfig pConfig);
}