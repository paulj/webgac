using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace WebGAC.Core {
  public class WebDAVParser {
    private const string DAV_NAMESPACE = "DAV:";

    public string[] ParsePropFindResponseForDirectories(XmlDocument pDoc, string pBase) {
      if (!pBase.StartsWith("/") || !pBase.EndsWith("/")) {
        throw new ArgumentException("Base must start and end with / - Provided: " + pBase);
      }

      XmlNodeList list = pDoc.SelectNodes("/D:multistatus/D:response/D:propstat/D:prop/D:resourcetype/D:collection/../../../../D:href", GetNamespaceManager(pDoc));
      List<string> result = new List<string>();

      foreach (XmlNode node in list) {
        string path = node.InnerXml;
        if (path.StartsWith(pBase)) {
          string remainingPath = path.Substring(pBase.Length);

          if (remainingPath.Length > 0) {
            result.Add(remainingPath);
          }
        }
      }

      return result.ToArray();
    }

    public string[] ParsePropFindResponseForFiles(XmlDocument pDoc, string pBase) {
      if (!pBase.StartsWith("/") || !pBase.EndsWith("/")) {
        throw new ArgumentException("Base must start and end with / - Provided: " + pBase);
      }

      XmlNodeList list =
        pDoc.SelectNodes(
          "/D:multistatus/D:response/D:propstat/D:prop/D:resourcetype[not(D:collection)]/../../../D:href",
          GetNamespaceManager(pDoc));
      List<string> result = new List<string>();

      foreach (XmlNode node in list) {
        string path = node.InnerXml;
        if (path.StartsWith(pBase)) {
          string remainingPath = path.Substring(pBase.Length);

          if (remainingPath.Length > 0) {
            result.Add(remainingPath);
          }
        }
      }

      return result.ToArray();
    }

    public void Parse(Stream pStream) {
      XmlReader reader = XmlReader.Create(pStream);
      while (reader.Read()) {
        
      }
    }

    protected XmlElement[] ParseMultiStatusResponses(XmlDocument pDoc) {
      

      // Retrieve the multistatus node, then each response node
      XmlNode node = FindChildByTagName(pDoc.ChildNodes, DAV_NAMESPACE, "multistatus");
      XmlNode[] childNodes = FindChildrenByTagName(pDoc.ChildNodes, DAV_NAMESPACE, "response");

      string a = node.Name;

      return null;
    }

    protected XmlNode FindChildByTagName(XmlNodeList pNodes, string pNamespaceUri, string pLocalName) {
      foreach (XmlNode node in pNodes) {
        if (node.LocalName == pLocalName && node.NamespaceURI == pNamespaceUri) {
          return node;
        }
      }

      throw new IndexOutOfRangeException(pLocalName + " is not a child node");
    }

    protected XmlNode[] FindChildrenByTagName(XmlNodeList pNodes, string pNamespaceUri, string pLocalName) {
      return null;
    }

    protected XmlNamespaceManager GetNamespaceManager(XmlDocument pDoc) {
      XmlNamespaceManager namespaces = new XmlNamespaceManager(pDoc.NameTable);
      namespaces.AddNamespace("D", DAV_NAMESPACE);

      return namespaces;
    }
  }
}
