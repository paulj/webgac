# WebGAC: .NET Binary Management made Easy

## Introduction
Managing binary dependencies in .NET can be a complicated task. For small projects,
checking the dependencies into source control tends to work just fine. So does requesting
that all developers have various binaries available in their GAC. Grow much bigger, or
add more projects, and managing that starts to get very difficult.

The Java world has had a solution to this problem for a long time, in the form of Maven and Ivy.
Remote servers store the binaries, and the build tool automatically downloads them on demand.
WebGAC adds this functionality to .NET, but without requiring you to switch build tools, or
maintain a separate configuration file. Dependencies are specified just the same way as normal,
but if you don't have them when building your project, WebGAC will fetch them for you automatically.

## Setting up your server
WebGAC requires a remote server that support WebDAV to store dependencies. Whilst this may sound like
a big requirement, it is actually just a few lines of configuration for a standard [Apache](http://httpd.apache.org)
web server.

1. Install an Apache web server somewhere
2. In your apache.conf file, create a virtual directory called "webgac" (note that you can use any name for
    this directory - these instructions just call it this for the sake of simplicity). The configuration for
    this virtual directory should look something like: <pre>DavLockDB /usr/local/apache2/var/DavLock
    &lt;Location /webgac>
      Dav On
    &lt;/Location></pre>
3. Ensure that within your web root, the directory webgac exists and is accessible by the user running Apache
4. Restart Apache
5. WARNING: At this stage, you've configured an open read/write Apache DAV server. This means that anyone with
   access to your server can read and write files. For anything more than local experimentation 
   (hopefully behind a firewall), please refer to the [mod_dav](http://httpd.apache.org/docs/2.0/mod/mod_dav.html)
   documentation for details on securing the server. The WebGAC client by default supports Basic authentication, so
   it is recommended to enable this.

## Setting up your clients (with the installer)
1. Download the latest installer from the [Downloads](http://github.com/paulj/webgac/downloads) section.
2. Run the installer
3. Load Visual Studio, and select Tools -> Configure WebGAC...
4. Select Add, and enter the address of your new WebGAC server, such as http://localhost/webgac
5. Ok the changes
6. Done!

## Making a binary available via the WebGAC
To make a binary available via the WebGAC, you'll need to upload the file. 
In this example, we'll walk through publishing the nunit.framework.dll file
via WebGAC.

1. Download the [NUnit binaries](http://launchpad.net/nunitv2/2.5/2.5.3/+download/NUnit-2.5.3.9345.zip)
2. In Visual Studio, select Tools -> Browse WebGAC...
3. Click "Upload..."
4. Click the ellipsis next to the file entry box, and select the nunit.framework.dll file.
5. Click "Upload"

## Enabling a Project to use WebGAC
Each project file that uses the WebGAC needs to have an additional set of MSBuild targets enabled to activate
WebGAC functionality. To do this, open your .csproj file in a text editor, and underneath the line
that reads `<Import Project="$(MSBuildBinPath)\Microsoft.CSharp.targets" />`, add a new line that
reads `<Import Project="$(MSBuildExtensionsPath)\WebGAC\WebGAC.targets" />`. Reload the project in Visual Studio.

## Adding a Dependency via the WebGAC
Finally, you can start enjoying the WebGAC. 

1. Right click on your project in the Visual Studio Solution Explorer, and select "Add WebGAC Reference..."
2. In the dialog that appears, nunit.framework should appear. Expand the node, and select the version. 
3. Click "Add"
4. The reference should be added to your project, and it should be ready to use!

## Software License
> Copyright (c) 2007-2010 Paul Jones <pauljones23@gmail.com>
> 
> Permission is hereby granted, free of charge, to any person
> obtaining a copy of this software and associated documentation
> files (the "Software"), to deal in the Software without
> restriction, including without limitation the rights to use, copy,
> modify, merge, publish, distribute, sublicense, and/or sell copies
> of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be
> included in all copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
> EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
> MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
> NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
> HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
> WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
> DEALINGS IN THE SOFTWARE.