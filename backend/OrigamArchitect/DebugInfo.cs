#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Origam.DA.Service;
using Origam.UI;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using Origam.OrigamEngine;
using Origam.Rule;

namespace Origam.Workflow;
/// <summary>
/// Summary description for DebugInfo.
/// </summary>
public class DebugInfo : IDebugInfoProvider
{
	private IDeploymentService _deployment = null;
	private readonly ResourceTools resourceTools = new ResourceTools(
		ServiceManager.Services.GetService<IBusinessServicesService>(),
		SecurityManager.CurrentUserProfile);
	
	public DebugInfo()
	{
	}
	#region Public Functions
	public string GetInfo()
	{
		try
		{
			_deployment = ServiceManager.Services.GetService(typeof(IDeploymentService)) as IDeploymentService;
		}
		catch{}
		SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
		StringBuilder result = new StringBuilder();
		// identity info
		AddSection("ORIGAM System Information", result);
		AddInfo("Local Time", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"), result);
		AddInfo("XSLT Date", MyXmlDateTime(DateTime.Now), result);
		// model info
		AddHeader("Model Repository Information", result);
        AddInfo("Model Format Version", VersionProvider.CurrentModelMetaVersion, result);
        IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		
		if(persistence == null)
		{
			AddLine("Not logged in to the model repository.", result);
		}
		AddHeader("Loaded Packages (Model Version/Deployed Version)", result);
		if( ! schema.IsSchemaLoaded)
		{
			AddLine("Model not loaded.", result);
		}
		else
		{
			try
			{
				IList<Package> packages = schema.ActiveExtension.IncludedPackages;
				packages.Add(schema.ActiveExtension);
				foreach(Package package in packages)
				{
					_deployment.CanUpdate(package);
					AddInfo(package.Name, package.VersionString + "/" + _deployment.CurrentDeployedVersion(package), result);
				}
			}
			catch
			{
				AddLine("Package references have been changed. Reload the model first.", result);
			}
		}
		AddHeader("Activated Features", result);
		if( ! schema.IsSchemaLoaded)
		{
			AddLine("Model not loaded.", result);
		}
		else
		{
			IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
			param.RefreshParameters();
			FeatureSchemaItemProvider p = schema.GetProvider(typeof(FeatureSchemaItemProvider)) as FeatureSchemaItemProvider;
			var features = p.ChildItems.ToList();
			features.Sort();
			foreach(Feature f in features)
			{
				if(param.IsFeatureOn(f.Name))
				{
					AddLine(f.Name, result);
				}
			}
		}
		// computer name, time
		AddHeader("Computer Information", result);
		AddInfo("Current Directory", Environment.CurrentDirectory, result);
		AddInfo("Machine Name", Environment.MachineName, result);
		AddInfo("OS Version", Environment.OSVersion, result);
		AddInfo("Product Version", Application.ProductVersion, result);
		AddInfo("CLR Version", Environment.Version, result);
		try
		{
			Assembly asm = Assembly.GetAssembly(typeof(System.Data.DataSet));
			
			System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location);
			string version = fvi.FileVersion;
			AddInfo("System.data.dll Version", version, result);
		}
		catch{}
		AddInfo("Current Culture", Application.CurrentCulture.EnglishName, result);
		AddInfo("Current Input Language", Application.CurrentInputLanguage.LayoutName, result);
		AddInfo("Memory Working Set", Environment.WorkingSet, result);
		AddInfo("System Directory", Environment.SystemDirectory, result);
		try
		{
			AddInfo("Executable Path", Application.ExecutablePath, result);
			AddInfo("Application Data Path", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), result);
			AddInfo("Startup Path", Application.StartupPath, result);
		}
		catch
		{}
		
		try
		{
			if(IsRemoteSession())
			{
				AddHeader("Terminal Server Information", result);
				AddInfo("Client Computer Name", GetTSClientName(WTS_CURRENT_SESSION), result);
				AddInfo("Client Computer Address", GetTSClientAddress(WTS_CURRENT_SESSION), result);
				AddLine(GetTSClientDisplay(WTS_CURRENT_SESSION), result);
			}
		}
		catch(Exception ex)
		{
			result.AppendFormat("The following exception occured while getting terminal session information: {0}{1}{2}", Environment.NewLine, ex.Message, Environment.NewLine);
		}
		// profile info
		AddHeader("Authentication Information", result);
		AddInfo("Identity Name", SecurityManager.CurrentPrincipal.Identity.Name, result);
		AddInfo("Identity Authentication Type", SecurityManager.CurrentPrincipal.Identity.AuthenticationType, result);
		
		IOrigamProfileProvider profileProvider = null;
		
		try
		{
			profileProvider = SecurityManager.GetProfileProvider();
		}
		catch(Exception ex)
		{
			AddInfo("Profile Provider", "Error loading profile provider. " + ex.Message, result);
		}
		if(profileProvider == null)
		{
			AddInfo("Profile Provider", "None", result);
		}
		else
		{
			AddInfo("Profile Provider", profileProvider.GetType().ToString(), result);
			try
			{
                UserProfile profile = SecurityManager.CurrentUserProfile();
                AddInfo("Profile Id", profile.Id, result);
				AddInfo("Profile Name", profile.FullName, result);
                AddInfo("Profile Email", profile.Email, result);
                AddInfo("Profile Resource Id", profile.ResourceId, result);
				AddInfo("Profile Business Unit Id", profile.BusinessUnitId, result);
				AddInfo("Profile Organization Id", profile.OrganizationId, result);
			}
			catch(Exception ex)
			{
				result.AppendFormat("The following exception occured while loading current user's profile: {0}{1}{2}", Environment.NewLine, ex.Message, Environment.NewLine); 
			}
		}
		// role info
		AddHeader("Role Information", result);
		try
		{
			IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
			if (authorizationProvider == null)
			{
				AddInfo("Authorization Provider", "None", result);
			}
			else
			{
				AddInfo("Authorization Provider", authorizationProvider.GetType().ToString(), result);
			}
		}
		catch(Exception ex)
		{
			result.AppendFormat("The following exception occured while loading Authorization:  {0}{1}{2}", Environment.NewLine, ex.Message, Environment.NewLine);
		}
		
		// resource info (from rule engine)
		AddHeader("Resource Management Information", result);
		try
		{
			AddInfo("Active Resource Id", resourceTools.ResourceIdByActiveProfile(), result);
		}
		catch(Exception ex)
		{
			AddLine("Could not get resource information. The following error occured:", result);
			AddLine(ex.Message, result);
		}
        // service info (connection strings, remote time, errors...)
        AddLine("", result);
		AddSection("Service Information", result);
		ServiceSchemaItemProvider services = schema.GetProvider(typeof(ServiceSchemaItemProvider)) as ServiceSchemaItemProvider;
		
		if(services == null)
		{
			AddLine("Model is not loaded. Services are not available.", result);
		}
		else
		{
			IBusinessServicesService serviceProvider = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
            foreach (Schema.WorkflowModel.Service service in services.ChildItems)
			{
				AddHeader(service.Name, result);
				try
				{
					IServiceAgent agent = serviceProvider.GetAgent(service.Name, RuleEngine.Create(null, null), null);
					AddLine(agent.Info, result);
				}
				catch(Exception ex)
				{
					AddLine("The following error occured while fetching service information:", result);
					AddLine(ex.Message, result);
				}
			}
		}
		return result.ToString();
	}
	#endregion
	#region Private Functions
	private const int WTS_CURRENT_SESSION = -1;
	public const int WTS_PROTOCOL_TYPE_CONSOLE =  0;    // Console
	public const int WTS_PROTOCOL_TYPE_ICA     =  1;    // ICA Protocol
	public const int WTS_PROTOCOL_TYPE_RDP     =  2;    // RDP Protocol
	public const int SM_REMOTESESSION		   = 0x1000;
	public struct WTS_CLIENT_ADDRESS 
	{
		public uint AddressFamily;  // AF_INET, AF_IPX, AF_NETBIOS, AF_UNSPEC
		public  Byte []Address;
	};
	public struct WTS_CLIENT_DISPLAY 
	{
		public uint HorizontalResolution; // horizontal dimensions, in pixels
		public uint VerticalResolution;   // vertical dimensions, in pixels
		public uint ColorDepth;           // 1=16, 2=256, 4=64K, 8=16M
	};
	private enum WTSInfoClass 
	{
		WTSInitialProgram,
		WTSApplicationName,
		WTSWorkingDirectory,
		WTSOEMId,
		WTSSessionId,
		WTSUserName,
		WTSWinStationName,
		WTSDomainName,
		WTSConnectState,
		WTSClientBuildNumber,
		WTSClientName,
		WTSClientDirectory,
		WTSClientProductId,
		WTSClientHardwareId,
		WTSClientAddress,
		WTSClientDisplay,
		WTSClientProtocolType,
	} ;
	[DllImport("user32.dll", EntryPoint=("GetSystemMetrics"))]
	public static extern bool GetSystemMetrics(int nIndex);
	[ DllImport( "wtsapi32.dll", EntryPoint = "WTSQuerySessionInformation", CallingConvention = CallingConvention.Cdecl ) ]
	private static extern bool WTSQuerySessionInformation(
		System.IntPtr hServer, 
		int sessionId, 
		WTSInfoClass wtsInfoClass,
		out System.IntPtr ppBuffer,
		out uint pBytesReturned);
	[ DllImport( "wtsapi32.dll", EntryPoint = "WTSFreeMemory", CallingConvention = CallingConvention.Cdecl ) ]
	private static extern void WTSFreeMemory(
		System.IntPtr ppBuffer);
	private bool IsRemoteSession()
	{
		return GetSystemMetrics( SM_REMOTESESSION );
	}
	private string GetTSClientProtocolType(int sessionID)
	{
		System.IntPtr  ppBuffer = System.IntPtr.Zero;
		uint pBytesReturned  = 0;
		int protocoleID;
		string protocoleType = "";
		if( WTSQuerySessionInformation(
			System.IntPtr.Zero,
			sessionID,
			WTSInfoClass.WTSClientProtocolType,
			out ppBuffer,
			out pBytesReturned) )
		{
			protocoleID = Marshal.ReadInt32(ppBuffer);
			switch(protocoleID)
			{
				case WTS_PROTOCOL_TYPE_CONSOLE:
					protocoleType = "Console";
					break;
				case WTS_PROTOCOL_TYPE_ICA:
					protocoleType = "ICA";
					break;
				case WTS_PROTOCOL_TYPE_RDP:
					protocoleType = "RDP";
					break;
				default:
					break;
			} 
		}
		WTSFreeMemory( ppBuffer );
		return protocoleType;
	}

	private string GetTSClientProductId(int sessionID)
	{
		System.IntPtr  ppBuffer = System.IntPtr.Zero;
		uint pBytesReturned  = 0;
		string productID = "";
		if( WTSQuerySessionInformation(
			System.IntPtr.Zero,
			sessionID,
			WTSInfoClass.WTSClientProductId,
			out ppBuffer,
			out pBytesReturned) )
		{
			productID = Marshal.PtrToStringAnsi(ppBuffer);
		}
		WTSFreeMemory( ppBuffer );
		return productID;
	}
	
	private string GetTSClientName(int sessionID)
	{
		System.IntPtr  ppBuffer = System.IntPtr.Zero;
		int adrBuffer;
		uint pBytesReturned  = 0;
		string clientName = "";
		if( WTSQuerySessionInformation(
			System.IntPtr.Zero,
			sessionID,
			WTSInfoClass.WTSClientName,
			out ppBuffer,
			out pBytesReturned) )
		{
			adrBuffer = (int)ppBuffer;
			clientName = Marshal.PtrToStringAnsi((System.IntPtr)adrBuffer);
		}
		WTSFreeMemory( ppBuffer );
		return clientName;
	}
	
	private string GetTSClientDisplay(int sessionID)
	{
		System.IntPtr  ppBuffer = System.IntPtr.Zero;
		uint pBytesReturned  = 0;
		StringBuilder sDisplay = new StringBuilder();
			
		// Interface DLL avec les structures
		WTS_CLIENT_DISPLAY clientDisplay = new WTS_CLIENT_DISPLAY();
		Type dataType = typeof(WTS_CLIENT_DISPLAY);
		if( WTSQuerySessionInformation(
			System.IntPtr.Zero,
			sessionID,
			WTSInfoClass.WTSClientDisplay,
			out ppBuffer,
			out pBytesReturned) )
		{
			clientDisplay = (WTS_CLIENT_DISPLAY)Marshal.PtrToStructure(ppBuffer,dataType);
			sDisplay.Append("Display Horizontal: " + (clientDisplay.HorizontalResolution).ToString() + Environment.NewLine);
			sDisplay.Append("Display Vertical: " + (clientDisplay.VerticalResolution).ToString() + Environment.NewLine);
			sDisplay.Append("Display Color Depth: " + (clientDisplay.ColorDepth).ToString());
		}
		WTSFreeMemory( ppBuffer );
		return sDisplay.ToString();
	}
	
	private string GetTSClientAddress(int sessionID)
	{
		System.IntPtr  ppBuffer = System.IntPtr.Zero;
		uint pBytesReturned  = 0;
		StringBuilder builder = new StringBuilder();
		// Interface avec API
		WTS_CLIENT_ADDRESS wtsAdr = new WTS_CLIENT_ADDRESS();
			
				
		if( WTSQuerySessionInformation(
			System.IntPtr.Zero,
			sessionID,
			WTSInfoClass.WTSClientAddress,
			out ppBuffer,
			out pBytesReturned) )
		{
			// ---------------------------------------------------------------------------------------------
			// Pour pouvoir r�cup�rer l'ensembe des informations du client connect�
			// ---------------------------------------------------------------------------------------------
			wtsAdr.Address = new Byte[pBytesReturned - 1];
			int run = (int)ppBuffer; // pointeur sur les donn�es
			Type t = typeof(Byte);
			Type t1 = typeof(uint);
			int uintSize = Marshal.SizeOf(t1);
			int byteSize = Marshal.SizeOf(t);
			
			//-- Lecture du type d'@
			wtsAdr.AddressFamily =(uint) Marshal.ReadInt32((System.IntPtr)run);
			
			//run+=uintSize;
			//run+=dataSize;			
			/*switch(wtsAdr.AddressFamily)
			{
				case 0:builder.Append("AF_UNSPEC");
					break;
				case 1:builder.Append("AF_INET");
					break;
				case 2:builder.Append("AF_IPX");
					break;
				case 3:builder.Append("AF_NETBIOS");
					break;
			}*/
			for(int i = 0;i<pBytesReturned-1;i++)
			{
				wtsAdr.Address[i] = Marshal.ReadByte((System.IntPtr)run);
				run += byteSize;
				// TO GET and to SEE ALL the DATA
				//builder.Append(wtsAdr.Address[i].ToString()+"-");
			}
			//builder.Append("-");
			builder.Append((wtsAdr.Address[4+2]).ToString());
			builder.Append(".");
			builder.Append((wtsAdr.Address[4+3]).ToString());
			builder.Append(".");
			builder.Append((wtsAdr.Address[4+4]).ToString());
			builder.Append(".");
			builder.Append((wtsAdr.Address[4+5]).ToString());
			// L'offset de 4 est du au fait que le type uint fait 4 Bytes et 
			// le type de connexion est pris dans l'ensemble des donn�es
		}
		WTSFreeMemory( ppBuffer );
		return builder.ToString();
	}
	void AddSection(string text, StringBuilder builder)
	{
		builder.Append("===============================================" + Environment.NewLine);
		builder.AppendFormat("====== {0}{1}", text, Environment.NewLine);
		builder.Append("===============================================" + Environment.NewLine);
	}
	void AddHeader(string text, StringBuilder builder)
	{
		builder.Append(Environment.NewLine);
		builder.Append(text);
		builder.AppendFormat("{0}{1}{2}", Environment.NewLine, "===============================================",	Environment.NewLine);
	}
	void AddLine(string text, StringBuilder builder)
	{
		builder.AppendFormat("{0}{1}", text, Environment.NewLine);
	}
	void AddInfo(string category, object data, StringBuilder builder)
	{
		string text;
		if(data == null)
		{
			text = "";
		}
		else
		{
			text = data.ToString();
		}
		builder.AppendFormat("{0}: {1}{2}", category, text, Environment.NewLine);
	}
	private string[] GetWindowsIdentityRoles( WindowsIdentity identity )
	{
		object result = typeof(WindowsIdentity).InvokeMember( "_GetRoles",
			BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
			null, identity, new object[]{identity.Token}, null );
		return (string[])result; 
	}
	private string MyXmlDateTime(DateTime date)
	{
		TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(date);
		int daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(date.Year).Delta.Hours;
		int hours = offset.Duration().Hours;
		int finalHours = hours; // + daylight;
		string sign = finalHours >= 0 ? "+" : "-";
		string result = date.ToString("yyyy-MM-dd") + "T00:00:00.0000000" + sign + finalHours.ToString("00") + ":" + offset.Minutes.ToString("00");
		return result;
	}
	#endregion
}
