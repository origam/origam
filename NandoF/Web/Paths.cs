#region  NandoF library -- Copyright 2005-2006 Nando Florestan
/*
This library is free software; you can redistribute it and/or modify
it under the terms of the Lesser GNU General Public License as published by
the Free Software Foundation; either version 2.1 of the License, or
(at your option) any later version.

This software is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this program; if not, see http://www.gnu.org/copyleft/lesser.html
 */
#endregion

// using HttpContext = System.Web.HttpContext;
using HttpRequest = System.Web.HttpRequest;
using Path        = System.IO.Path;

namespace NandoF.Web
{
	// Class inspired by the articles:
	// 1. http://www.awprofessional.com/articles/article.asp?p=101145&rl=1
	// 2. "Making sense of ASP.Net Paths" by Rick Strahl:
	//    http://west-wind.com/weblog/posts/269.aspx
	public class Paths
	{
		//Rick Strahl said:
		//kc, you should never use a server name in your path. In fact,
		//if you can always use relative paths that start from the
		//application directory if possible or if you're using
		//code use ResolveUrl as described above with the ~.
		//These things all ensure that if you move the app to the server that
		//it will just run without any changes.
		//At the very least use server relative paths but never a full HTTP url
		//as the one above. ie. use /projectname/somepage.aspx.
		//But ideally you'd use: ~/somepage.aspx either in a control that
		//understands this or alternately with the Page.ResolveUrl() method.
		
		static HttpRequest request {
			get { return System.Web.HttpContext.Current.Request; }
		}
		
		static public string CombineVirtual (string part1, string part2) {
			return System.IO.Path.Combine(part1, part2).Replace('\\', '/');
		}
		
		static public bool VirtualFileExists(string virtualPath)  {
			return System.IO.File.Exists(request.MapPath(virtualPath));
		}
		
		static public bool VirtualDirExists (string virtualDirectory)  {
			return System.IO.Directory.Exists(request.MapPath(virtualDirectory));
		}
		
		/// <summary>Examples:
		/// http://localhost/myapp/subdir/client.aspx/extra
		/// http://localhost/myapp/subdir/client.aspx?sku=WWHELP30</summary>
		/// <returns>A System.Uri containing the fully qualified URL:
		/// protocol, domain (or IP), path, file and extra path</returns>
		static public System.Uri  VirtualAllUri  {
			get  { return request.Url; }
		}
		
		/// <summary>Examples:
		/// http://localhost/myapp/subdir/client.aspx/extra
		/// http://localhost/myapp/subdir/client.aspx?sku=WWHELP30</summary>
		/// <returns>A string containing the fully qualified URL:
		/// protocol, domain (or IP), path, file and extra path</returns>
		static public string VirtualAll  {
			get { return request.Url.ToString(); }
		}
		
		/// <summary>Example:  /myapp/subdir/pathsample.aspx/extra</summary>
		/// <returns>A string containing application, path, file and querystring
		/// </returns>
		static public string VirtualAppPathFileExtra {
			get  {
				return request.RawUrl;
				// return request.Path;
			}
		}
		
		// Virtual path to the currently executing script, minus the webserver root
		// example:  /MyWebApp/admin/Item.aspx
		/// <summary>Gets a string containing application, path and file.
		/// Example: /myapp/subdir/pathsample.aspx</summary>
		static public string VirtualAppPathFile  {
			get  {
				return request.FilePath;
				// return request.CurrentExecutionFilePath;
			}
		}
		
		/// <summary>Example: /myapp/subdir/</summary>
		/// <returns>A string containing the virtual application and path of the
		/// current page, without the script name.</returns>
		static public string VirtualAppPath  {
			get  {
				return request.FilePath
					.Substring (0, request.Path.LastIndexOf("/") + 1);
			}
		}
		
		/// <summary>Gets a string containing the query string or extra path info.
		/// In other words, the part of the URL after the file.</summary>
		static public string Extra {
			get { return request.PathInfo; }
		}
		
		/// <summary>Returns the application path. Example: /myapp</summary>
		static public string VirtualApp  {
			get { return request.ApplicationPath; }
		}
		
//			public string VirtualProtocolDomainApp  {
//				get { return request.Url.GetLeftPart(UriPartial.Authority) +
//						this.ResolveUrl( Request.ApplicationPath);
//			}
		
		/// <summary>Example: /myapp/subdir</summary>
		/// <param name="c">The Control whose directory we want to know</param>
		/// <returns>A string containing the virtual application and folder
		/// where the control is. Very useful if you need to know the location of
		/// your ASCX control instead of the location of the page.</returns>
		static public string GetVirtualAppPath(System.Web.UI.Control c)  {
			return c.TemplateSourceDirectory;
		}
		
		/// <summary>Example: /myapp/subdir</summary>
		/// <param name="p">The Page whose directory we want to know</param>
		/// <returns>A string containing the virtual application and folder
		/// where the page is.</returns>
		static public string GetVirtualAppPath(System.Web.UI.Page p)  {
			return p.TemplateSourceDirectory;
		}
		
		/// <summary>Converts a virtual path into the corresponding physical path.
		/// </summary>
		/// <param name="virtualPath">A string such as "log.txt"</param>
		/// <returns>Example: c:\mywebdirs\myapp\subdir\log.txt</returns>
		static public string PhysicalFromVirtual (string virtualPath) {
			return request.MapPath(virtualPath);
		}
		
		/// <summary>Example:  c:\mywebdirs\myapp\subdir\pathsample.aspx
		/// </summary><returns>Physical path to the requested URL
		/// (currently executing .aspx)</returns>
		static public string PhysicalAppPathFile   {
			get { return request.PhysicalPath; }
		}
		
		/// <summary>Example: c:\mywebdirs\myapp\subdir\ </summary>
		/// <returns>The physical folder where the .aspx is.</returns>
		static public string PhysicalAppPath  {
			get { return Path.GetDirectoryName(PhysicalAppPathFile); }
		}
		
		/// <summary>Example: c:\mywebdirs\myapp\ </summary>
		/// <returns>A string containing the physical path to the application root.
		/// </returns>
		static public string PhysicalApp  {
			get { return request.PhysicalApplicationPath; }
		}
		
		static public string Port  {
			get {
				string port = request.ServerVariables["SERVER_PORT"];
				if (port == null || port == "80" || port == "443")
					return string.Empty;
				else return port;
			}
		}
		
		static public string Protocol {
			get {
				string protocol = request.ServerVariables["SERVER_PORT_SECURE"];
				if (protocol == null || protocol == "0") return "http://";
				else return "https://";
			}
		}
		
		static public string DomainOrIp {
			get { return request.ServerVariables["SERVER_NAME"]; }
		}
		
		static public string ProtocolDomainPort {
			get {
				string port = (Port==string.Empty) ? string.Empty : ":"+Port;
				return Protocol + DomainOrIp + port;
			}
		}
		
		static public string ProtocolDomainPortApp {
			get {
				return CombineVirtual(ProtocolDomainPort, VirtualApp);
			}
		}
		
		/// <summary>Substitutes a tilde in the given virtual path by its meaning:
		/// the virtual application path.</summary>
		/// <remarks>Example: "~/myPath" becomes "/myApp/myPath".</remarks>
		static public string ResolveTildeIn(string virtualPath)  {
			if (Text.IsNullOrEmpty(virtualPath) ||
			    !virtualPath.StartsWith("~/"))     return virtualPath;
			return CombineVirtual(VirtualApp, virtualPath.Substring(2));
		}
	}
}

