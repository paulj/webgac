using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace WebGAC.Core.Test {
  [TestFixture]
  public class WebDAVParserTest {
    private WebDAVParser mParser;

    [SetUp]
    public void SetUp() {
      mParser = new WebDAVParser();
    }

    [Test]
    public void ShouldReturnValidListOfDirectoriesFromResponseWithValidProperties() {
      ForEachFormat(sValidPropertyListWithOneDir, delegate(XmlDocument doc) {
        string[] result = mParser.ParsePropFindResponseForDirectories(doc, "/webgac/");
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("test/", result[0]);
      });
    }

    [Test]
    public void ShouldReturnEmptyListOfDirectoriesFromResponseWithNoListedDirectories() {
      ForEachFormat(sValidSubDirPropertyListWithNoDirs, delegate(XmlDocument doc) {
        string[] result = mParser.ParsePropFindResponseForDirectories(doc, "/webgac/");
        Assert.AreEqual(0, result.Length);
      });
    }

    [Test]
    public void ShouldReturnValidListOfFiles() {
      ForEachFormat(sValidFileList, delegate(XmlDocument doc) {
        string[] result = mParser.ParsePropFindResponseForFiles(doc, "/webgac/MetaSharp.Web.APIs/0.1.7338.1_Debug/");
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual("MetaSharp.Web.APIs.dll", result[0]);
        Assert.AreEqual("MetaSharp.Web.APIs.pdb", result[1]);
      });
    }

    private delegate void FormatTester(XmlDocument doc);
    private void ForEachFormat(string content, FormatTester tester) {
      // Test for Apache style output (no hostname in hrefs)
      XmlDocument apacheDoc = new XmlDocument();
      apacheDoc.LoadXml(content);
      tester(apacheDoc);

      // Test for IIS style output (hostname in hrefs)
      XmlDocument iisDoc = new XmlDocument();
      iisDoc.LoadXml(content.Replace("/webgac", "http://localhost/webgac"));
      tester(iisDoc);
    }

    private const string sValidPropertyListWithOneDir =
      @"<?xml version=""1.0"" encoding=""utf-8""?>
<D:multistatus xmlns:D=""DAV:"" xmlns:ns0=""DAV:"">
  <D:response xmlns:lp1=""DAV:"">
    <D:href>/webgac/</D:href>
    <D:propstat>
      <D:prop>
        <lp1:resourcetype><D:collection /></lp1:resourcetype>
        <D:getcontenttype>httpd/unix-directory</D:getcontenttype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response xmlns:lp1=""DAV:"">
    <D:href>/webgac/test/</D:href>
    <D:propstat>
      <D:prop>
        <lp1:resourcetype><D:collection /></lp1:resourcetype>
        <D:getcontenttype>httpd/unix-directory</D:getcontenttype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
</D:multistatus>";
    private const string sValidSubDirPropertyListWithNoDirs =
      @"<?xml version=""1.0"" encoding=""utf-8""?>
        <D:multistatus xmlns:D=""DAV:"" xmlns:ns0=""DAV:"">
          <D:response xmlns:lp1=""DAV:"">
            <D:href>/webgac/test/</D:href>
            <D:propstat>
              <D:prop>
                <lp1:resourcetype><D:collection/></lp1:resourcetype>
                <D:getcontenttype>httpd/unix-directory</D:getcontenttype>
              </D:prop>
            <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
        </D:multistatus>";

    private const string sValidFileList =
      @"<?xml version=""1.0"" encoding=""utf-8""?>
        <D:multistatus xmlns:D=""DAV:"" xmlns:ns0=""DAV:"">
          <D:response xmlns:lp1=""DAV:"">
            <D:href>/webgac/MetaSharp.Web.APIs/0.1.7338.1_Debug/</D:href>
            <D:propstat>
              <D:prop>
                <lp1:resourcetype>
                  <D:collection />
                </lp1:resourcetype>
                <D:getcontenttype>httpd/unix-directory</D:getcontenttype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response xmlns:lp1=""DAV:"">
            <D:href>/webgac/MetaSharp.Web.APIs/0.1.7338.1_Debug/MetaSharp.Web.APIs.dll</D:href>
            <D:propstat>
              <D:prop>
                <lp1:resourcetype />
                <D:getcontenttype>application/octet-stream</D:getcontenttype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response xmlns:lp1=""DAV:"">
            <D:href>/webgac/MetaSharp.Web.APIs/0.1.7338.1_Debug/MetaSharp.Web.APIs.pdb</D:href>
            <D:propstat>
              <D:prop>
                <lp1:resourcetype />
                <D:getcontenttype>chemical/x-pdb</D:getcontenttype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
        </D:multistatus>";
  }
}
