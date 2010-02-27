using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace WebGAC.Core {
  /// <summary>
  /// Utility class for maintaing the WebGAC.
  /// </summary>
  public class WebGAC {
    private readonly WebGACConfig mConfig;
    private readonly IDictionary<string, WebDAVClient> mClients;
    private CredentialsRequestHandler mCredRequestHandler;

    public WebGAC() {
      mConfig = new WebGACConfig();
      mClients = new Dictionary<string, WebDAVClient>();

      WebGACConfig userConfig = LoadUserConfig();
      if (userConfig != null) {
        mConfig.Merge(userConfig);
      }

      // Set defaults on our config if we don't have any
      if (mConfig.LocalStore == null) {
        mConfig.LocalStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebGAC\\Repository");
      }
      if (mConfig.Repositories == null) {
        mConfig.Repositories = new string[0];
      }
      if (mConfig.AuthenticatedRepositories == null) {
        mConfig.AuthenticatedRepositories = new AuthenticatedRepository[0];
      }

      // Subscribe to updates on our config
      Config.RepositoriesUpdated += Config_RepositoriesUpdated;
    }

    public event WebGACErrorHandler WebGACErrors;

    public WebGACConfig Config {
      get { return mConfig; }
    }

    public CredentialsRequestHandler CredRequestHandler {
      get { return mCredRequestHandler; }
      set { mCredRequestHandler = value; }
    }

    public void TestRepositoryUrl(string pRepository) {
      // Attempt to list the contents of the repository url
      WebDAVClient client = GetWebDAVClient(new AuthenticatedRepository(pRepository, null, null));
      client.ListDirectories();
    }

    public string Resolve(string pName, Version pVersion, string pPublicKeyToken, string pProcessorArchitecture, string pPrimaryConfiguration, string[] pAllConfigurations) {
      AssemblyInfo info = new AssemblyInfo(pName, pVersion, pPublicKeyToken, pProcessorArchitecture);

      return Resolve(info, pPrimaryConfiguration, pAllConfigurations);
    }

    /// <summary>
    /// Requests that the given assembly be resolved, either locally or remotely.
    /// </summary>
    /// <param name="pAssemblyInfo">detail of the assembly, such as 
    /// &quot;Castle.ActiveRecord, Version=1.0.3.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc, processorArchitecture=MSIL&quot;</param>
    /// <param name="pPrimaryConfiguration">the default configuration to look for</param>
    /// <param name="pAllConfigurations">all of the configurations to search, in case the default configuration did not contain the requested artifact</param>
    /// <returns>the path to the assembly, or null if the assembly cannot be found</returns>
    public string Resolve(string pAssemblyInfo, string pPrimaryConfiguration, string[] pAllConfigurations) {
      AssemblyInfo info = new AssemblyInfo(pAssemblyInfo);

      // Check we have a name and version
      if (info.Name == null || info.Version == null) {
        return null;
      }

      return Resolve(info, pPrimaryConfiguration, pAllConfigurations);
    }

    /// <summary>
    /// Retrieves the list of available assemblies.
    /// </summary>
    /// <returns>the list of available assemblies</returns>
    public string[] GetAllAssemblies() {
      SortedDictionary<string, object> sorted = new SortedDictionary<string, object>();

      // List all the local assemblies
      if (Directory.Exists(Config.LocalStore)) {
        string[] directories = Directory.GetDirectories(Config.LocalStore);
        foreach (string directory in directories) {
          string assemblyName = Path.GetFileName(directory);

          if (!sorted.ContainsKey(assemblyName)) {
            sorted.Add(assemblyName, null);
          }
        }
      }

      // Start listing remote assemblies
      foreach (AuthenticatedRepository repository in Config.AllRepositories) {
        try {
          WebDAVClient client = GetWebDAVClient(repository);

          string[] repositoryItems = client.ListDirectories();
          foreach (string repositoryItem in repositoryItems) {
            string assemblyName = CleanRepositoryItem(repositoryItem);

            if (!sorted.ContainsKey(assemblyName)) {
              sorted.Add(assemblyName, null);
            }
          }
        } catch (Exception ex) {
          FireWebGACError("GetAllAssemblies", ex);
        }
      }

      List<string> result = new List<string>(sorted.Keys);
      return result.ToArray();
    }

    public Version[] GetAllVersions(string pAssemblyName, bool pAllowLocalReferences) {
      SortedDictionary<Version, object> sorted = new SortedDictionary<Version, object>();

      // List all the local assemblies
      if (pAllowLocalReferences) {
        string assemblyPath = Path.Combine(Config.LocalStore, pAssemblyName);
        if (Directory.Exists(assemblyPath)) {
          string[] directories = Directory.GetDirectories(assemblyPath);
          foreach (string directory in directories) {
            Version version = GetVersionFromDirectory(directory);

            if (!sorted.ContainsKey(version)) {
              sorted.Add(version, null);
            }
          }
        }
      }

      // Start listing remote assemblies
      foreach (AuthenticatedRepository repository in Config.AllRepositories) {
        try {
          WebDAVClient client = GetWebDAVClient(repository);

          if (client.CheckCollectionExists(pAssemblyName)) {
            string[] repositoryItems = client.ListDirectories(pAssemblyName);
            foreach (string repositoryItem in repositoryItems) {
              Version version = GetVersionFromDirectory(CleanRepositoryItem(repositoryItem));

              if (!sorted.ContainsKey(version)) {
                sorted.Add(version, null);
              }
            }
          }
        } catch (Exception ex) {
          FireWebGACError("GetAllVersions", ex);
        }
      }

      List<Version> result = new List<Version>(sorted.Keys);
      result.Reverse();
      return result.ToArray();
    }

    public void StoreLocal(string pAssemblyDetails, string pFilePath, string pConfiguration) {
      AssemblyInfo info = new AssemblyInfo(pAssemblyDetails);
      if (!info.IsStrongSigned) {
        throw new ArgumentException("Assembly " + pFilePath + " is not strong-signed.");
      }

      string installPath = GetLocalPath(info, Path.GetExtension(pFilePath), pConfiguration);
      string targetDir = Path.GetDirectoryName(installPath);
      if (!Directory.Exists(targetDir)) {
        Directory.CreateDirectory(targetDir);
      }

      File.Copy(pFilePath, installPath, true);
    }

    public void StoreRemote(string pAssemblyDetails, string pFilePath, string pConfiguration, string pRepository) {
      AssemblyInfo info = new AssemblyInfo(pAssemblyDetails);
      if (!info.IsStrongSigned) {
        throw new ArgumentException("Assembly " + pFilePath + " is not strong-signed.");
      }

      // Attempt to find a repository match
      AuthenticatedRepository authRepos = FindAuthenticatedRepository(pRepository);

      WebDAVClient client = GetWebDAVClient(authRepos);
      string collectionName = string.Format("{0}/{1}_{2}", info.Name, info.Version, pConfiguration);
      string fileName = string.Format("{0}{1}", info.Name, Path.GetExtension(pFilePath));

      client.UploadFile(collectionName, fileName, pFilePath);
    }

    /// <summary>
    /// Saves the current configuration in the user config file.
    /// </summary>
    public void SaveUserConfig() {
      SaveConfig(UserConfigLocation, mConfig);
    }

    private AuthenticatedRepository FindAuthenticatedRepository(string pRepositoryUrl) {
      foreach (AuthenticatedRepository repos in Config.AllRepositories) {
        if (repos.Url.ToLower() == pRepositoryUrl.ToLower()) {
          return repos;
        }
      }

      return new AuthenticatedRepository(pRepositoryUrl, null, null);
    }

    private string Resolve(AssemblyInfo pInfo, string pPrimaryConfiguration, string[] pAllConfigurations) {
      foreach (string configuration in GetConfigurationSearchOrder(pPrimaryConfiguration, pAllConfigurations)) {
        // Check if we have the file in our local repository
        string localPath = GetLocalPath(pInfo, ".dll", configuration);
        if (File.Exists(localPath)) {
          return localPath;
        }

        // We didn't have it locally. Try to resolve it remotely from our list of repositories.
        foreach (AuthenticatedRepository repository in mConfig.AllRepositories) {
          WebDAVClient client = GetWebDAVClient(repository);

          string collectionName = string.Format("{0}/{1}_{2}", pInfo.Name, pInfo.Version, configuration);
          string fileName = string.Format("{0}.dll", pInfo.Name);

          if (client.CheckFileExists(collectionName, fileName)) {
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));
//            client.DownloadFile(collectionName, fileName, localPath);
            client.DownloadCollection(collectionName, Path.GetDirectoryName(localPath));

            return localPath;
          }
        }
      }

      return null;
    }

    private static IEnumerable<string> GetConfigurationSearchOrder(string pPrimary, string[] pAll) {
      yield return pPrimary;

      foreach (string config in pAll) {
        if (pPrimary != config) {
          yield return config;
        }
      }
    }

    private string GetLocalPath(AssemblyInfo pInfo, string pExtension, string pConfiguration) {
      return Path.Combine(mConfig.LocalStore, string.Format("{0}{3}{1}_{2}{3}{0}{4}", pInfo.Name, pInfo.Version, pConfiguration, Path.DirectorySeparatorChar, pExtension));
    }

    private static string GetRemotePath(string pRepositoryUrl, AssemblyInfo pInfo) {
      return UrlCombine(pRepositoryUrl, string.Format("{0}/{1}/{0}.dll", pInfo.Name, pInfo.Version));
    }

    private static string UrlCombine(string pBase, string pChild) {
      if (pBase.EndsWith("/") != pChild.StartsWith("/")) {
        return pBase + pChild;
      } else if (pBase.EndsWith("/") && pChild.StartsWith("/")) {
        return pBase + pChild.Substring(1);
      } else {
        return pBase + "/" + pChild;
      }
    }

    private void Config_RepositoriesUpdated(WebGACConfig pConfig) {
      mClients.Clear();
    }

    private WebDAVClient GetWebDAVClient(AuthenticatedRepository pRepository) {
      lock (mClients) {
        if (mClients.ContainsKey(pRepository.ToString())) {
          return mClients[pRepository.ToString()];
        }

        string storeName = pRepository.Url.Replace(":", "").Replace('/', '_') + ".xml";
        WebDAVClient client = new WebDAVClient(pRepository.Url, new WebDAVCache(GetLocationForCacheFile, storeName), pRepository.Username, pRepository.Password);
        client.CredentialHandler = CredRequestHandler;

        // Cache the client
        mClients[pRepository.ToString()] = client;

        return client;
      }
    }

    private string GetLocationForCacheFile(string pCollection) {
      string path = Path.Combine(mConfig.LocalStore, pCollection.Replace('/', Path.DirectorySeparatorChar));
      if (!Directory.Exists(path)) {
        Directory.CreateDirectory(path);
      }

      return path;
    }

    private void FireWebGACError(string pOperation, Exception pDetail) {
      WebGACErrorHandler handler = WebGACErrors;
      if (handler != null) {
        handler(pOperation, pDetail);
      }
    }

    private static Version GetVersionFromDirectory(string pDir) {
      return new Version(Path.GetFileName(pDir).Split('_')[0]);
    }

    /// <summary>
    /// Loads the user-specific configuration for the WebGAC.
    /// </summary>
    /// <returns></returns>
    private static WebGACConfig LoadUserConfig() {
      return LoadConfig(UserConfigLocation);
    }

    /// <summary>
    /// Loads the given WebGAC config.
    /// </summary>
    /// <param name="pConfigPath">the path to the configuration file</param>
    /// <returns>the loaded config, or null if the file doesn't exist</returns>
    private static WebGACConfig LoadConfig(string pConfigPath) {
      if (!File.Exists(pConfigPath)) {
        return null;
      }

      XmlSerializer serializer = new XmlSerializer(typeof(WebGACConfig));
      FileStream fs = new FileStream(pConfigPath, FileMode.Open, FileAccess.Read);
      try {
        return (WebGACConfig) serializer.Deserialize(fs);
      } finally {
        fs.Close();
      }
    }

    /// <summary>
    /// Saves the given WebGAC config.
    /// </summary>
    /// <param name="pConfigPath">the path to the configuration file</param>
    /// <param name="pConfig">the config to save</param>
    private static void SaveConfig(string pConfigPath, WebGACConfig pConfig) {
      // Make sure the directories exist
      string configDir = Path.GetDirectoryName(pConfigPath);
      if (!Directory.Exists(configDir)) {
        Directory.CreateDirectory(configDir);
      }

      // Serialise the config
      XmlSerializer serializer = new XmlSerializer(typeof(WebGACConfig));
      FileStream fs = new FileStream(pConfigPath, FileMode.Create, FileAccess.Write);
      try {
        serializer.Serialize(fs, pConfig);
      } finally {
        fs.Close();
      }

      // Save out the targets file
      File.WriteAllText(Path.Combine(configDir, "WebGAC.targets.user"),
        string.Format(
          "<Project DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n" +
	        "  <PropertyGroup>\n" +
		      "    <WebGACLocalStore>{0}</WebGACLocalStore>\n" +
	        "  </PropertyGroup>\n" +
          "</Project>",
          pConfig.LocalStore));
    }

    private static string UserConfigLocation {
      get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebGAC\\settings.xml"); }
    }

    private static string CleanRepositoryItem(string pName) {
      return pName.Trim('/');
    }
  }

  public class AssemblyInfo {
    private readonly NameValueCollection mDetails;
    private readonly Version mVersion;
    
    public AssemblyInfo(string pAssemblyFullName) {
      mDetails = ParseAssemblyDetails(pAssemblyFullName);

      if (mDetails["Version"] != null) {
        mVersion = new Version(mDetails["Version"]);
      } else {
        mVersion = null;
      }
    }

    public AssemblyInfo(string pName, Version pVersion, string pPublicKeyToken, string pProcessorArchitecture) {
      mDetails = new NameValueCollection();
      mDetails["Name"] = pName;
      mVersion = pVersion;
      mDetails["PublicKeyToken"] = pPublicKeyToken;
      mDetails["processorArchitecture"] = pProcessorArchitecture;
    }

    public string Name {
      get { return mDetails["Name"]; }
    }

    public Version Version {
      get { return mVersion; }
    }

    public bool IsStrongSigned {
      get { return Version != null; }
    }

    private static NameValueCollection ParseAssemblyDetails(string pInfo) {
      NameValueCollection result = new NameValueCollection();
      string[] nameOther = pInfo.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
      if (nameOther.Length == 0) {
        return result;
      }

      result.Add("Name", nameOther[0]);

      if (nameOther.Length > 1) {
        foreach (string item in nameOther[1].Split(',')) {
          string[] pair = item.Split(new char[] { '=' }, 2);

          result.Add(pair[0].Trim(), pair[1].Trim());
        }
      }

      return result;
    }
  }
}