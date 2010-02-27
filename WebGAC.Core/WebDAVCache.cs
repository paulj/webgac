using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebGAC.Core {
  /// <summary>
  /// Delegate used to inform the cache where the cache for a given file is stored.
  /// </summary>
  /// <param name="pCollection"></param>
  /// <returns></returns>
  public delegate string CacheLocationDelegate(string pCollection);

  public class WebDAVCache {
    private readonly object mMonitor = new object();
    private readonly CacheLocationDelegate mCacheLocation;
    private readonly string mFileName;

    public WebDAVCache(CacheLocationDelegate pCacheLocation, string pFileName) {
      mCacheLocation = pCacheLocation;
      mFileName = pFileName;
    }

    public void StoreContent(string pCollection, string pContent) {
      lock (mMonitor) {
        File.WriteAllText(GetCacheLocation(pCollection), pContent);
      }
    }

    public string GetContent(string pCollection) {
      lock (mMonitor) {
        string location = GetCacheLocation(pCollection);
        FileInfo info = new FileInfo(location);
        if (!info.Exists || info.LastWriteTime < DateTime.Now - new TimeSpan(0, 0, 10, 0)) {
          return null;
        }

        return File.ReadAllText(location);
      }
    }

    public void Invalidate(string pCollection) {
      lock (mMonitor) {
        File.Delete(GetCacheLocation(pCollection));
      }
    }

    private string GetCacheLocation(string pCollection) {
      return Path.Combine(mCacheLocation(pCollection), mFileName);
    }
  }
}
