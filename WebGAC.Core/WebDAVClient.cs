using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Xml;

namespace WebGAC.Core {
  /// <summary>
  /// Delegate used to handle updates to credentials.
  /// </summary>
  /// <param name="pSender"></param>
  /// <param name="pAuhType"></param>
  /// <param name="pUsername"></param>
  /// <param name="pPassword"></param>
  public delegate void CredentialsUpdateHandler(
    WebDAVClient pSender, string pAuhType, string pUsername, string pPassword);

  /// <summary>
  /// Delegate used to request credentials.
  /// </summary>
  /// <param name="pRequestUri">the uri the credentials are for</param>
  /// <param name="pAuthType">the authentication type (eg Basic, Digest)</param>
  /// <param name="pAuthParams">the authentication parameters (eg, Realm)</param>
  /// <param name="pIsFirst">whether this is the first request in the session (ie, whether the previous credentials have been rejected)</param>
  /// <returns>the credentials, or none if they are not provided</returns>
  public delegate NetworkCredential CredentialsRequestHandler(Uri pRequestUri, string pAuthType, NameValueCollection pAuthParams, bool pIsFirst);

  public class WebDAVClient : ICredentials {
    /// <summary>
    /// WebClient used to store credentials and proxy configurations.
    /// </summary>
    private readonly WebClient mWebClient;
    private readonly string mServerBase;
    private readonly Uri mServerBaseUri;
    private readonly WebDAVParser mParser;
    private readonly string mBasePath;
    private readonly WebDAVCache mDavCache;
    private readonly string mDefaultUserName;
    private readonly string mDefaultPassword;
    private int mRetryCount = 3;
    private CredentialsRequestHandler mCredentialHandler;
    private CredentialCache mCache;
    private string mLastAuthString;

    public WebDAVClient(string pServerBase, WebDAVCache pDavCache, string pDefaultUserName, string pDefaultPassword) {
      mServerBase = pServerBase;
      mDavCache = pDavCache;
      mDefaultUserName = pDefaultUserName;
      mDefaultPassword = pDefaultPassword;

      mWebClient = new WebClient();
      mServerBaseUri = new Uri(pServerBase);
      mParser = new WebDAVParser();
      
      if (mServerBaseUri.AbsolutePath.EndsWith("/")) {
        mBasePath = mServerBaseUri.AbsolutePath;
      } else {
        mBasePath = mServerBaseUri.AbsolutePath + "/";
      }

      mCache = new CredentialCache();

      // Store the credentials
//      CredentialCache credentials = new CredentialCache();
//      mWebClient.Credentials = credentials;
//      credentials.Add(mServerBaseUri.Host, mServerBaseUri.Port, pAuthType, new NetworkCredential(pUsername, pPassword));
    }

    /// <summary>
    /// Event indicating that the bound list of credentials has been updated.
    /// </summary>
    public event CredentialsUpdateHandler CredentialsUpdated;

    public CredentialsRequestHandler CredentialHandler {
      get { return mCredentialHandler; }
      set { mCredentialHandler = value; }
    }

    /// <summary>
    /// The repository this client connects to.
    /// </summary>
    public string ServerBase {
      get { return mServerBase; }
    }

    public string[] ListDirectories() {
        return ListDirectories("");
    }

    public string[] ListDirectories(string pCollection) {
      string davResponse = GetDirectoryListing(pCollection);
      string basePath = mBasePath + (pCollection.TrimEnd('/') + "/").TrimStart('/');

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(davResponse);

      return mParser.ParsePropFindResponseForDirectories(doc, basePath);
    }

    public string[] ListFiles(string pCollection) {
      string davResponse = GetDirectoryListing(pCollection);
      string basePath = mBasePath + (pCollection.TrimEnd('/') + "/").TrimStart('/');

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(davResponse);

      return mParser.ParsePropFindResponseForFiles(doc, basePath);
    }

    public void CreateCollection(string pCollection, bool pRecurse) {
      List<string> createRequired = new List<string>();
      if (pRecurse) {
        // Check for directories. Start at the deepest, then work back until we find one that exists.
        foreach (string colName in GetCollectionHierarchyInReverse(pCollection)) {
          if (CheckCollectionExists(colName)) {
            break;
          } else {
            createRequired.Add(colName);
          }
        }

        // Reverse the create required list.
        createRequired.Reverse();
      } else {
        createRequired.Add(pCollection);
      }

      foreach (string collection in createRequired) {
        Uri requestUri = new Uri(mServerBaseUri, collection);

        mDavCache.Invalidate(GetParentCollection(collection));

        PerformAuthenticated<object>(requestUri,
                                     delegate(HttpWebRequest pRequest) {
                                       pRequest.Method = "MKCOL";

                                       HttpWebResponse response = (HttpWebResponse) pRequest.GetResponse();
                                       response.Close();
                                       return null;
                                     });
      }
    }

    public bool CheckCollectionExists(string pCollection) {
      Uri requestUri = new Uri(mServerBaseUri, pCollection.TrimEnd('/') + "/");

      // Work out if a collection exists by checking if each individual directory exists
      string[] hierarchy = GetCollectionHierarchy(pCollection);
      if (hierarchy.Length == 0) {  // If they are requesting the root directory
        return true;
      }

      // Get the list of child directories for "root" entry
      string[] childDirs = ListDirectories();
      string previousHierarchy = "";
      for (int i = 0; i < hierarchy.Length; ++i) {
        bool found = false;
        
        // Find this hierarchy element in the parent list
        for (int j = 0; !found && j < childDirs.Length; ++j) {
          if (previousHierarchy + childDirs[j] == hierarchy[i]) {
            found = true;
          }
        }

        // If we didn't find anything, then give up
        if (!found) {
          return false;
        }

        // Get the next set of directories
        childDirs = ListDirectories(hierarchy[i]);
        previousHierarchy = hierarchy[i];
      }

      return true;

      /*return PerformAuthenticated<bool>(requestUri,
                                         delegate(HttpWebRequest pRequest) {
                                           pRequest.Method = "PROPFIND";
                                           pRequest.ContentType = "text/xml";
                                           pRequest.Headers["Depth"] = "1";
                                           StreamWriter writer = new StreamWriter(pRequest.GetRequestStream(), Encoding.UTF8);
                                           writer.Write(NOPROPS_REQ);
                                           writer.Close();

                                           try {
                                             HttpWebResponse response = (HttpWebResponse) pRequest.GetResponse();
                                             response.Close();
                                             return true;
                                           } catch (WebException ex) {
                                             if (((HttpWebResponse) ex.Response).StatusCode == HttpStatusCode.NotFound) {
                                               return false;
                                             }

                                             throw;
                                           }
                                         });*/
    }

    public bool CheckFileExists(string pCollection, string pFileName) {
//      Uri requestUri = new Uri(mServerBaseUri, pCollection.TrimEnd('/') + "/" + pFileName);

      // First, check if the parent collection exists
      if (!CheckCollectionExists(pCollection)) {
        return false;
      }

      // Next, list the files in the parent directory, and then see if the filename is in the list
      string[] filenames = ListFiles(pCollection);
      foreach (string filename in filenames) {
        if (pFileName == filename) {
          return true;
        }
      }

      return false;
      /*return PerformAuthenticated<bool>(requestUri,
                                         delegate(HttpWebRequest pRequest) {
                                           pRequest.Method = "HEAD";
                                           
                                           try {
                                             HttpWebResponse response = (HttpWebResponse)pRequest.GetResponse();
                                             response.Close();
                                             return true;
                                           } catch (WebException ex) {
                                             if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound) {
                                               return false;
                                             }

                                             throw;
                                           }
                                         });*/
    }

    public bool DownloadFile(string pCollection, string pFileName, string pLocalName) {
      Uri requestUri = new Uri(mServerBaseUri, pCollection.TrimEnd('/') + "/" + pFileName);

      return PerformAuthenticated<bool>(requestUri,
                                         delegate(HttpWebRequest pRequest) {
                                           pRequest.Method = "GET";

                                           try {
                                             HttpWebResponse response = (HttpWebResponse)pRequest.GetResponse();

                                             try {
                                               string tmpName = pLocalName + ".tmp";
                                               byte[] buffer = new byte[10000];
                                               FileStream fileWriter =
                                                 new FileStream(tmpName, FileMode.Create, FileAccess.ReadWrite);
                                               try {
                                                 Stream responseStream = response.GetResponseStream();
                                                 int bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                                                 while (bytesRead > 0) {
                                                   fileWriter.Write(buffer, 0, bytesRead);

                                                   bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                                                 }

                                                 fileWriter.Close();

                                                 if (File.Exists(pLocalName)) {
                                                   File.Delete(pLocalName);
                                                 }
                                                 File.Move(tmpName, pLocalName);
                                               } catch {
                                                 // If we get an error, make sure we don't leave the temp file behind
                                                 fileWriter.Close();
                                                 File.Delete(tmpName);

                                                 throw;
                                               }
                                             } finally {
                                               response.Close();
                                             }
                                             return true;
                                           } catch (WebException ex) {
                                             if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound) {
                                               return false;
                                             }

                                             throw;
                                           }
                                         });
    }

    public void DownloadCollection(string pCollection, string pLocalDirectory) {
      string[] contents = ListFiles(pCollection);
      foreach (string contentFile in contents) {
        DownloadFile(pCollection, contentFile, Path.Combine(pLocalDirectory, contentFile));
      }
    }

    public void UploadFile(string pCollection, string pFileName, string pLocalName) {
      // Ensure the collection exists
      CreateCollection(pCollection, true);

      // Invalidate the cache
      mDavCache.Invalidate(pCollection);

      // Put the file
      Uri requestUri = new Uri(mServerBaseUri, pCollection.TrimEnd('/') + "/" + pFileName);

      PerformAuthenticated<object>(requestUri,
                                   delegate(HttpWebRequest pRequest) {
                                     pRequest.Method = "PUT";

                                     FileInfo localInfo = new FileInfo(pLocalName);
                                     pRequest.ContentLength = localInfo.Length;

                                     Stream requestStream = pRequest.GetRequestStream();
                                     FileStream fs = new FileStream(pLocalName, FileMode.Open, FileAccess.Read);
                                     try {
                                       byte[] buffer = new byte[10000];
                                       int bytesRead = fs.Read(buffer, 0, buffer.Length);
                                       while (bytesRead > 0) {
                                         requestStream.Write(buffer, 0, bytesRead);

                                         bytesRead = fs.Read(buffer, 0, buffer.Length);
                                       }
                                     } finally {
                                       fs.Close();
                                     }

                                     HttpWebResponse response = (HttpWebResponse)pRequest.GetResponse();
                                     response.Close();

                                     return null;
                                   });
    }

    #region ICredentials Members
    public NetworkCredential GetCredential(Uri uri, string authType) {
      NetworkCredential result = mCache.GetCredential(uri.Host.ToLower(), uri.Port, authType.ToLower());
      if (result != null) {
        return result;
      }

      return null;
    }
    #endregion

    public IEnumerable<string> GetCollectionHierarchyInReverse(string pFullName) {
      string[] items = GetCollectionHierarchy(pFullName);

      // Reverse iterate it
      for (int i = items.Length - 1; i >= 0; --i) {
        // Build the string
        yield return items[i];
      }
    }

    public string GetParentCollection(string pCollection) {
      if (pCollection == string.Empty) {
        return string.Empty;
      }

      string[] items = GetCollectionHierarchy(pCollection);
      if (items.Length < 2) {
        return string.Empty;
      }

      // Return the second last element
      return items[items.Length - 2];
    }

    public string[] GetCollectionHierarchy(string pFullName) {
      // Break up the components
      string[] components = pFullName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

      // Build the full list.
      List<string> items = new List<string>();
      StringBuilder current = new StringBuilder();
      if (pFullName.StartsWith("/")) {
        current.Append("/");
      }
      for (int i = 0; i < components.Length; ++i) {
        current.Append(components[i]);
        current.Append("/");
        items.Add(current.ToString());
      }

      return items.ToArray();
    }

    private string GetDirectoryListing(string pCollection) {
      Uri requestUri = new Uri(mServerBaseUri, (pCollection.TrimEnd('/') + "/").TrimStart('/'));
      
      string davResponse = mDavCache.GetContent(pCollection);
      if (davResponse == null) {
        davResponse = PerformAuthenticated<string>(requestUri,
                                                   delegate(HttpWebRequest pRequest) {
                                                     pRequest.Method = "PROPFIND";
                                                     pRequest.ContentType = "text/xml";
                                                     pRequest.Headers["Depth"] = "1";
                                                     StreamWriter writer =
                                                       new StreamWriter(pRequest.GetRequestStream(), Encoding.UTF8);
                                                     writer.Write(ALLPROPS_REQ);
                                                     writer.Close();

                                                     HttpWebResponse response = (HttpWebResponse)pRequest.GetResponse();
                                                     try {
                                                       return new StreamReader(response.GetResponseStream()).ReadToEnd();
                                                     } finally {
                                                       response.Close();
                                                     }
                                                   });
        mDavCache.StoreContent(pCollection, davResponse);
      }

      return davResponse;
    }

    private delegate T AuthenticatedWebDelegate<T>(HttpWebRequest pRequest);

    private T PerformAuthenticated<T>(Uri pUri, AuthenticatedWebDelegate<T> pDelegate) {
      Uri currentUri = pUri;
      Uri lastUri = null;

      for (int i = 0; i < mRetryCount; ++i) {
        HttpWebRequest request = WebRequest.Create(currentUri) as HttpWebRequest;
        if (request == null) {
          throw new ArgumentException("Uri " + pUri + " is not a valid HTTP URI.");
        }
        request.Credentials = this;
        request.AllowAutoRedirect = true;
        request.PreAuthenticate = true;
        if (mLastAuthString != null) {
          request.Headers[HttpRequestHeader.Authorization] = mLastAuthString;
        }

        try {
          T result = pDelegate(request);

          string lastAuth = request.Headers[HttpRequestHeader.Authorization];
          if (lastAuth != null && lastAuth.ToLower().StartsWith("basic")) {
            // Store the last basic authentication string that we sent
            mLastAuthString = request.Headers[HttpRequestHeader.Authorization];
          }

          return result;
        } catch (WebException ex) {
          HttpWebResponse invalidResponse = ex.Response as HttpWebResponse;
          if (invalidResponse != null) {
            // Update our uri
            currentUri = invalidResponse.ResponseUri;

            // The delegate rejected the request. Maybe we need to retry with authentication.
            if (invalidResponse.StatusCode == HttpStatusCode.Unauthorized) {
              string authenticateHeader = invalidResponse.Headers[HttpResponseHeader.WwwAuthenticate];
              if (i >= mRetryCount - 1 || !GetAuthentication(currentUri, authenticateHeader, lastUri != currentUri)) {
                // The user cancelled, or we ran out of attempts. Throw an unauthorized exception.
                throw;
              }

              // Store what our last uri was
              lastUri = currentUri;
            } else {
              throw;
            }
          } else {
            throw;
          }
        }
      }

      // Should never reach here anyway.
      throw new ApplicationException("Method should not have reached this point");
    }

    private bool GetAuthentication(Uri pUri, string pAuthHeader, bool isFirst) {
      // Sometimes we don't get an auth header. So in those cases, there isn't anything we can do
      if (pAuthHeader == null) {
        return false;
      }

      string[] parts = pAuthHeader.Split(new char[] { ' ' }, 2);
      string authType = parts[0];
      NameValueCollection authParams = new NameValueCollection();
      foreach (string authParam in QuoteAwareSplit(' ', parts[1], StringSplitOptions.RemoveEmptyEntries)) {
        string[] paramParts = authParam.Split(new char[] { '=' }, 2);
        authParams.Add(paramParts[0], paramParts[1]);
      }
      

      if (CredentialHandler != null) {
        NetworkCredential creds = CredentialHandler(pUri, authType, authParams, isFirst);
        if (creds != null) {
          if (mCache.GetCredential(pUri.Host.ToLower(), pUri.Port, authType.ToLower()) != null) {
            mCache.Remove(pUri.Host.ToLower(), pUri.Port, authType.ToLower());
          }
          mCache.Add(pUri.Host.ToLower(), pUri.Port, authType.ToLower(), creds);
          return true;
        }
      }
      if (mDefaultUserName != null && mDefaultPassword != null) {
        mCache.Add(pUri.Host.ToLower(), pUri.Port, authType.ToLower(), new NetworkCredential(mDefaultUserName, mDefaultPassword));
        return true;
      }

      return false;
    }

    private Uri GetCollectionUri(string pCollection) {
      return new Uri(mServerBaseUri, pCollection.TrimEnd('/') + "/");
    }

    private static string[] QuoteAwareSplit(char pSplitChar, IEnumerable<char> pTarget, StringSplitOptions pSplitOptions) {
      List<string> result = new List<string>();
      StringBuilder current = new StringBuilder();
      bool inQuotes = false;

      foreach (char c in pTarget) {
        if (c == pSplitChar && !inQuotes) {
          if (current.Length > 0 || (pSplitOptions & StringSplitOptions.RemoveEmptyEntries) == 0) {
            result.Add(current.ToString());
          }

          current = new StringBuilder();
        } else if (c == '"') {
          inQuotes = !inQuotes;
        } else {
          current.Append(c);
        }
      }

      if (current.Length > 0) {
        result.Add(current.ToString());
      }

      return result.ToArray();
    }

    private const string ALLPROPS_REQ =
      @"<?xml version=""1.0"" encoding=""utf-8""?>
        <D:propfind xmlns:D=""DAV:"">
          <D:prop>
            <D:resourcetype/>
            <D:getcontenttype/>
          </D:prop>
        </D:propfind>
      ";
    private const string NOPROPS_REQ =
      @"<?xml version=""1.0"" encoding=""utf-8""?>
        <D:propfind xmlns:D=""DAV:"">
          <D:prop />
        </D:propfind>
      ";

    /*
PROPFIND /webgac/ HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Depth: 1
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress
Content-type: text/xml
Content-length: 345

<?xml version="1.0"?>
<A:propfind xmlns:A="DAV:">
    <A:prop>
        <A:displayname/>
        <A:resourcetype/>
        <A:getcontenttype/>
        <A:getcontentlength/>
        <A:getlastmodified/>
        <A:lockdiscovery/>
        <A:checked-in/>
        <A:checked-out/>
        <A:version-name/>
    </A:prop>
</A:propfind>
HTTP/1.1 207 Multi-Status
Date: Fri, 23 Nov 2007 11:49:13 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Content-Length: 702
Content-Type: text/xml; charset="utf-8"

<?xml version="1.0" encoding="utf-8"?>
<D:multistatus xmlns:D="DAV:" xmlns:ns0="DAV:">
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Thu, 22 Nov 2007 21:44:13 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
</D:multistatus>
MKCOL /webgac/test HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress

HTTP/1.1 201 Created
Date: Fri, 23 Nov 2007 11:53:57 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Location: http://dev.example.com:8080/webgac/test
Content-Length: 351
Content-Type: text/html

<!DOCTYPE HTML PUBLIC "-//IETF//DTD HTML 2.0//EN">
<html><head>
<title>201 Created</title>
</head><body>
<h1>Created</h1>
<p>Collection /webgac/test has been created.</p>
<hr />
<address>Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4 Server at dev.example.com Port 8080</address>
</body></html>
PROPFIND /webgac/ HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Depth: 1
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress
Content-type: text/xml
Content-length: 345

<?xml version="1.0"?>
<A:propfind xmlns:A="DAV:">
    <A:prop>
        <A:displayname/>
        <A:resourcetype/>
        <A:getcontenttype/>
        <A:getcontentlength/>
        <A:getlastmodified/>
        <A:lockdiscovery/>
        <A:checked-in/>
        <A:checked-out/>
        <A:version-name/>
    </A:prop>
</A:propfind>
HTTP/1.1 207 Multi-Status
Date: Fri, 23 Nov 2007 11:53:58 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Content-Length: 1305
Content-Type: text/xml; charset="utf-8"

<?xml version="1.0" encoding="utf-8"?>
<D:multistatus xmlns:D="DAV:" xmlns:ns0="DAV:">
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:53:57 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/test/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:53:57 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
</D:multistatus>
PROPFIND /webgac/test/ HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Depth: 1
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress
Content-type: text/xml
Content-length: 345

<?xml version="1.0"?>
<A:propfind xmlns:A="DAV:">
    <A:prop>
        <A:displayname/>
        <A:resourcetype/>
        <A:getcontenttype/>
        <A:getcontentlength/>
        <A:getlastmodified/>
        <A:lockdiscovery/>
        <A:checked-in/>
        <A:checked-out/>
        <A:version-name/>
    </A:prop>
</A:propfind>
HTTP/1.1 207 Multi-Status
Date: Fri, 23 Nov 2007 11:54:03 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Content-Length: 707
Content-Type: text/xml; charset="utf-8"

<?xml version="1.0" encoding="utf-8"?>
<D:multistatus xmlns:D="DAV:" xmlns:ns0="DAV:">
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/test/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:53:57 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
</D:multistatus>
DELETE /webgac/test/ HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress

HTTP/1.1 204 No Content
Date: Fri, 23 Nov 2007 11:54:09 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Content-Length: 0
Content-Type: httpd/unix-directory

PROPFIND /webgac/ HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Depth: 1
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress
Content-type: text/xml
Content-length: 345

<?xml version="1.0"?>
<A:propfind xmlns:A="DAV:">
    <A:prop>
        <A:displayname/>
        <A:resourcetype/>
        <A:getcontenttype/>
        <A:getcontentlength/>
        <A:getlastmodified/>
        <A:lockdiscovery/>
        <A:checked-in/>
        <A:checked-out/>
        <A:version-name/>
    </A:prop>
</A:propfind>
HTTP/1.1 207 Multi-Status
Date: Fri, 23 Nov 2007 11:54:09 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Content-Length: 702
Content-Type: text/xml; charset="utf-8"

<?xml version="1.0" encoding="utf-8"?>
<D:multistatus xmlns:D="DAV:" xmlns:ns0="DAV:">
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:54:09 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
</D:multistatus>
MKCOL /webgac/test HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress

HTTP/1.1 201 Created
Date: Fri, 23 Nov 2007 11:54:16 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Location: http://dev.example.com:8080/webgac/test
Content-Length: 351
Content-Type: text/html

<!DOCTYPE HTML PUBLIC "-//IETF//DTD HTML 2.0//EN">
<html><head>
<title>201 Created</title>
</head><body>
<h1>Created</h1>
<p>Collection /webgac/test has been created.</p>
<hr />
<address>Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4 Server at dev.example.com Port 8080</address>
</body></html>
PROPFIND /webgac/ HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Depth: 1
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress
Content-type: text/xml
Content-length: 345

<?xml version="1.0"?>
<A:propfind xmlns:A="DAV:">
    <A:prop>
        <A:displayname/>
        <A:resourcetype/>
        <A:getcontenttype/>
        <A:getcontentlength/>
        <A:getlastmodified/>
        <A:lockdiscovery/>
        <A:checked-in/>
        <A:checked-out/>
        <A:version-name/>
    </A:prop>
</A:propfind>
HTTP/1.1 207 Multi-Status
Date: Fri, 23 Nov 2007 11:54:17 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Content-Length: 1305
Content-Type: text/xml; charset="utf-8"

<?xml version="1.0" encoding="utf-8"?>
<D:multistatus xmlns:D="DAV:" xmlns:ns0="DAV:">
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:54:16 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/test/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:54:16 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
</D:multistatus>
PROPFIND /webgac/test/ HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Depth: 1
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress
Content-type: text/xml
Content-length: 345

<?xml version="1.0"?>
<A:propfind xmlns:A="DAV:">
    <A:prop>
        <A:displayname/>
        <A:resourcetype/>
        <A:getcontenttype/>
        <A:getcontentlength/>
        <A:getlastmodified/>
        <A:lockdiscovery/>
        <A:checked-in/>
        <A:checked-out/>
        <A:version-name/>
    </A:prop>
</A:propfind>
HTTP/1.1 207 Multi-Status
Date: Fri, 23 Nov 2007 11:54:19 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Content-Length: 707
Content-Type: text/xml; charset="utf-8"

<?xml version="1.0" encoding="utf-8"?>
<D:multistatus xmlns:D="DAV:" xmlns:ns0="DAV:">
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/test/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:54:16 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
</D:multistatus>
PROPFIND /webgac/ HTTP/1.1
Host: dev.example.com:8080
Connection: TE
TE: trailers, deflate, gzip, compress
User-Agent: UCI DAV Explorer/0.91 RPT-HTTPClient/0.3-3E
Depth: 1
Translate: f
Authorization: Basic cGpvbmVzOmQ5bHVnby1u
Accept-Encoding: deflate, gzip, x-gzip, compress, x-compress
Content-type: text/xml
Content-length: 345

<?xml version="1.0"?>
<A:propfind xmlns:A="DAV:">
    <A:prop>
        <A:displayname/>
        <A:resourcetype/>
        <A:getcontenttype/>
        <A:getcontentlength/>
        <A:getlastmodified/>
        <A:lockdiscovery/>
        <A:checked-in/>
        <A:checked-out/>
        <A:version-name/>
    </A:prop>
</A:propfind>
HTTP/1.1 207 Multi-Status
Date: Fri, 23 Nov 2007 11:54:23 GMT
Server: Apache/2.0.59 (Win32) DAV/2 mod_ssl/2.0.59 OpenSSL/0.9.7j SVN/1.4.4 mod_python/3.3.1 Python/2.4.4
Content-Length: 1305
Content-Type: text/xml; charset="utf-8"

<?xml version="1.0" encoding="utf-8"?>
<D:multistatus xmlns:D="DAV:" xmlns:ns0="DAV:">
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:54:16 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
<D:response xmlns:lp1="DAV:" xmlns:lp2="http://apache.org/dav/props/" xmlns:g0="DAV:">
<D:href>/webgac/test/</D:href>
<D:propstat>
<D:prop>
<lp1:resourcetype><D:collection/></lp1:resourcetype>
<D:getcontenttype>httpd/unix-directory</D:getcontenttype>
<lp1:getlastmodified>Fri, 23 Nov 2007 11:54:16 GMT</lp1:getlastmodified>
<D:lockdiscovery/>
</D:prop>
<D:status>HTTP/1.1 200 OK</D:status>
</D:propstat>
<D:propstat>
<D:prop>
<g0:displayname/>
<g0:getcontentlength/>
<g0:checked-in/>
<g0:checked-out/>
<g0:version-name/>
</D:prop>
<D:status>HTTP/1.1 404 Not Found</D:status>
</D:propstat>
</D:response>
</D:multistatus>

     */
  }
}
  ;