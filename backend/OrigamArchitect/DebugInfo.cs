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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using Origam.OrigamEngine;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Workflow;

/// <summary>
/// Summary description for DebugInfo.
/// </summary>
public class DebugInfo : IDebugInfoProvider
{
    private IDeploymentService _deployment = null;
    private readonly ResourceTools resourceTools = new ResourceTools(
        businessService: ServiceManager.Services.GetService<IBusinessServicesService>(),
        userProfileGetter: SecurityManager.CurrentUserProfile
    );

    public DebugInfo() { }

    #region Public Functions
    public string GetInfo()
    {
        try
        {
            _deployment =
                ServiceManager.Services.GetService(serviceType: typeof(IDeploymentService))
                as IDeploymentService;
        }
        catch { }
        SchemaService schema =
            ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
        StringBuilder result = new StringBuilder();
        // identity info
        AddSection(text: "ORIGAM System Information", builder: result);
        AddInfo(
            category: "Local Time",
            data: DateTime.Now.ToString(format: "yyyy-MM-ddTHH:mm:ss.fffffffzzz"),
            builder: result
        );
        AddInfo(category: "XSLT Date", data: MyXmlDateTime(date: DateTime.Now), builder: result);
        // model info
        AddHeader(text: "Model Repository Information", builder: result);
        AddInfo(
            category: "Model Format Version",
            data: VersionProvider.CurrentModelMetaVersion,
            builder: result
        );
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;

        if (persistence == null)
        {
            AddLine(text: "Not logged in to the model repository.", builder: result);
        }
        AddHeader(text: "Loaded Packages (Model Version/Deployed Version)", builder: result);
        if (!schema.IsSchemaLoaded)
        {
            AddLine(text: "Model not loaded.", builder: result);
        }
        else
        {
            try
            {
                IList<Package> packages = schema.ActiveExtension.IncludedPackages;
                packages.Add(item: schema.ActiveExtension);
                foreach (Package package in packages)
                {
                    _deployment.CanUpdate(extension: package);
                    AddInfo(
                        category: package.Name,
                        data: package.VersionString
                            + "/"
                            + _deployment.CurrentDeployedVersion(extension: package),
                        builder: result
                    );
                }
            }
            catch
            {
                AddLine(
                    text: "Package references have been changed. Reload the model first.",
                    builder: result
                );
            }
        }
        AddHeader(text: "Activated Features", builder: result);
        if (!schema.IsSchemaLoaded)
        {
            AddLine(text: "Model not loaded.", builder: result);
        }
        else
        {
            IParameterService param =
                ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
                as IParameterService;
            param.RefreshParameters();
            FeatureSchemaItemProvider p =
                schema.GetProvider(type: typeof(FeatureSchemaItemProvider))
                as FeatureSchemaItemProvider;
            var features = p.ChildItems.ToList();
            features.Sort();
            foreach (Feature f in features)
            {
                if (param.IsFeatureOn(featureCode: f.Name))
                {
                    AddLine(text: f.Name, builder: result);
                }
            }
        }
        // computer name, time
        AddHeader(text: "Computer Information", builder: result);
        AddInfo(category: "Current Directory", data: Environment.CurrentDirectory, builder: result);
        AddInfo(category: "Machine Name", data: Environment.MachineName, builder: result);
        AddInfo(category: "OS Version", data: Environment.OSVersion, builder: result);
        AddInfo(category: "Product Version", data: Application.ProductVersion, builder: result);
        AddInfo(category: "CLR Version", data: Environment.Version, builder: result);
        try
        {
            Assembly asm = Assembly.GetAssembly(type: typeof(System.Data.DataSet));

            System.Diagnostics.FileVersionInfo fvi =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(fileName: asm.Location);
            string version = fvi.FileVersion;
            AddInfo(category: "System.data.dll Version", data: version, builder: result);
        }
        catch { }
        AddInfo(
            category: "Current Culture",
            data: Application.CurrentCulture.EnglishName,
            builder: result
        );
        AddInfo(
            category: "Current Input Language",
            data: Application.CurrentInputLanguage.LayoutName,
            builder: result
        );
        AddInfo(category: "Memory Working Set", data: Environment.WorkingSet, builder: result);
        AddInfo(category: "System Directory", data: Environment.SystemDirectory, builder: result);
        try
        {
            AddInfo(category: "Executable Path", data: Application.ExecutablePath, builder: result);
            AddInfo(
                category: "Application Data Path",
                data: Environment.GetFolderPath(folder: Environment.SpecialFolder.ApplicationData),
                builder: result
            );
            AddInfo(category: "Startup Path", data: Application.StartupPath, builder: result);
        }
        catch { }

        try
        {
            if (IsRemoteSession())
            {
                AddHeader(text: "Terminal Server Information", builder: result);
                AddInfo(
                    category: "Client Computer Name",
                    data: GetTSClientName(sessionID: WTS_CURRENT_SESSION),
                    builder: result
                );
                AddInfo(
                    category: "Client Computer Address",
                    data: GetTSClientAddress(sessionID: WTS_CURRENT_SESSION),
                    builder: result
                );
                AddLine(text: GetTSClientDisplay(sessionID: WTS_CURRENT_SESSION), builder: result);
            }
        }
        catch (Exception ex)
        {
            result.AppendFormat(
                format: "The following exception occured while getting terminal session information: {0}{1}{2}",
                arg0: Environment.NewLine,
                arg1: ex.Message,
                arg2: Environment.NewLine
            );
        }
        // profile info
        AddHeader(text: "Authentication Information", builder: result);
        AddInfo(
            category: "Identity Name",
            data: SecurityManager.CurrentPrincipal.Identity.Name,
            builder: result
        );
        AddInfo(
            category: "Identity Authentication Type",
            data: SecurityManager.CurrentPrincipal.Identity.AuthenticationType,
            builder: result
        );

        IOrigamProfileProvider profileProvider = null;

        try
        {
            profileProvider = SecurityManager.GetProfileProvider();
        }
        catch (Exception ex)
        {
            AddInfo(
                category: "Profile Provider",
                data: "Error loading profile provider. " + ex.Message,
                builder: result
            );
        }
        if (profileProvider == null)
        {
            AddInfo(category: "Profile Provider", data: "None", builder: result);
        }
        else
        {
            AddInfo(
                category: "Profile Provider",
                data: profileProvider.GetType().ToString(),
                builder: result
            );
            try
            {
                UserProfile profile = SecurityManager.CurrentUserProfile();
                AddInfo(category: "Profile Id", data: profile.Id, builder: result);
                AddInfo(category: "Profile Name", data: profile.FullName, builder: result);
                AddInfo(category: "Profile Email", data: profile.Email, builder: result);
                AddInfo(category: "Profile Resource Id", data: profile.ResourceId, builder: result);
                AddInfo(
                    category: "Profile Business Unit Id",
                    data: profile.BusinessUnitId,
                    builder: result
                );
                AddInfo(
                    category: "Profile Organization Id",
                    data: profile.OrganizationId,
                    builder: result
                );
            }
            catch (Exception ex)
            {
                result.AppendFormat(
                    format: "The following exception occured while loading current user's profile: {0}{1}{2}",
                    arg0: Environment.NewLine,
                    arg1: ex.Message,
                    arg2: Environment.NewLine
                );
            }
        }
        // role info
        AddHeader(text: "Role Information", builder: result);
        try
        {
            IOrigamAuthorizationProvider authorizationProvider =
                SecurityManager.GetAuthorizationProvider();
            if (authorizationProvider == null)
            {
                AddInfo(category: "Authorization Provider", data: "None", builder: result);
            }
            else
            {
                AddInfo(
                    category: "Authorization Provider",
                    data: authorizationProvider.GetType().ToString(),
                    builder: result
                );
            }
        }
        catch (Exception ex)
        {
            result.AppendFormat(
                format: "The following exception occured while loading Authorization:  {0}{1}{2}",
                arg0: Environment.NewLine,
                arg1: ex.Message,
                arg2: Environment.NewLine
            );
        }

        // resource info (from rule engine)
        AddHeader(text: "Resource Management Information", builder: result);
        try
        {
            AddInfo(
                category: "Active Resource Id",
                data: resourceTools.ResourceIdByActiveProfile(),
                builder: result
            );
        }
        catch (Exception ex)
        {
            AddLine(
                text: "Could not get resource information. The following error occured:",
                builder: result
            );
            AddLine(text: ex.Message, builder: result);
        }
        // service info (connection strings, remote time, errors...)
        AddLine(text: "", builder: result);
        AddSection(text: "Service Information", builder: result);
        ServiceSchemaItemProvider services =
            schema.GetProvider(type: typeof(ServiceSchemaItemProvider))
            as ServiceSchemaItemProvider;

        if (services == null)
        {
            AddLine(text: "Model is not loaded. Services are not available.", builder: result);
        }
        else
        {
            IBusinessServicesService serviceProvider =
                ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
                as IBusinessServicesService;
            foreach (Schema.WorkflowModel.Service service in services.ChildItems)
            {
                AddHeader(text: service.Name, builder: result);
                try
                {
                    IServiceAgent agent = serviceProvider.GetAgent(
                        serviceType: service.Name,
                        ruleEngine: RuleEngine.Create(contextStores: null, transactionId: null),
                        workflowEngine: null
                    );
                    AddLine(text: agent.Info, builder: result);
                }
                catch (Exception ex)
                {
                    AddLine(
                        text: "The following error occured while fetching service information:",
                        builder: result
                    );
                    AddLine(text: ex.Message, builder: result);
                }
            }
        }
        return result.ToString();
    }
    #endregion
    #region Private Functions
    private const int WTS_CURRENT_SESSION = -1;
    public const int WTS_PROTOCOL_TYPE_CONSOLE = 0; // Console
    public const int WTS_PROTOCOL_TYPE_ICA = 1; // ICA Protocol
    public const int WTS_PROTOCOL_TYPE_RDP = 2; // RDP Protocol
    public const int SM_REMOTESESSION = 0x1000;

    public struct WTS_CLIENT_ADDRESS
    {
        public uint AddressFamily; // AF_INET, AF_IPX, AF_NETBIOS, AF_UNSPEC
        public Byte[] Address;
    };

    public struct WTS_CLIENT_DISPLAY
    {
        public uint HorizontalResolution; // horizontal dimensions, in pixels
        public uint VerticalResolution; // vertical dimensions, in pixels
        public uint ColorDepth; // 1=16, 2=256, 4=64K, 8=16M
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
    };

    [DllImport(dllName: "user32.dll", EntryPoint = ("GetSystemMetrics"))]
    public static extern bool GetSystemMetrics(int nIndex);

    [DllImport(
        dllName: "wtsapi32.dll",
        EntryPoint = "WTSQuerySessionInformation",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern bool WTSQuerySessionInformation(
        System.IntPtr hServer,
        int sessionId,
        WTSInfoClass wtsInfoClass,
        out System.IntPtr ppBuffer,
        out uint pBytesReturned
    );

    [DllImport(
        dllName: "wtsapi32.dll",
        EntryPoint = "WTSFreeMemory",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern void WTSFreeMemory(System.IntPtr ppBuffer);

    private bool IsRemoteSession()
    {
        return GetSystemMetrics(nIndex: SM_REMOTESESSION);
    }

    private string GetTSClientProtocolType(int sessionID)
    {
        System.IntPtr ppBuffer = System.IntPtr.Zero;
        uint pBytesReturned = 0;
        int protocoleID;
        string protocoleType = "";
        if (
            WTSQuerySessionInformation(
                hServer: System.IntPtr.Zero,
                sessionId: sessionID,
                wtsInfoClass: WTSInfoClass.WTSClientProtocolType,
                ppBuffer: out ppBuffer,
                pBytesReturned: out pBytesReturned
            )
        )
        {
            protocoleID = Marshal.ReadInt32(ptr: ppBuffer);
            switch (protocoleID)
            {
                case WTS_PROTOCOL_TYPE_CONSOLE:
                {
                    protocoleType = "Console";
                    break;
                }

                case WTS_PROTOCOL_TYPE_ICA:
                {
                    protocoleType = "ICA";
                    break;
                }

                case WTS_PROTOCOL_TYPE_RDP:
                {
                    protocoleType = "RDP";
                    break;
                }

                default:
                {
                    break;
                }
            }
        }
        WTSFreeMemory(ppBuffer: ppBuffer);
        return protocoleType;
    }

    private string GetTSClientProductId(int sessionID)
    {
        System.IntPtr ppBuffer = System.IntPtr.Zero;
        uint pBytesReturned = 0;
        string productID = "";
        if (
            WTSQuerySessionInformation(
                hServer: System.IntPtr.Zero,
                sessionId: sessionID,
                wtsInfoClass: WTSInfoClass.WTSClientProductId,
                ppBuffer: out ppBuffer,
                pBytesReturned: out pBytesReturned
            )
        )
        {
            productID = Marshal.PtrToStringAnsi(ptr: ppBuffer);
        }
        WTSFreeMemory(ppBuffer: ppBuffer);
        return productID;
    }

    private string GetTSClientName(int sessionID)
    {
        System.IntPtr ppBuffer = System.IntPtr.Zero;
        int adrBuffer;
        uint pBytesReturned = 0;
        string clientName = "";
        if (
            WTSQuerySessionInformation(
                hServer: System.IntPtr.Zero,
                sessionId: sessionID,
                wtsInfoClass: WTSInfoClass.WTSClientName,
                ppBuffer: out ppBuffer,
                pBytesReturned: out pBytesReturned
            )
        )
        {
            adrBuffer = (int)ppBuffer;
            clientName = Marshal.PtrToStringAnsi(ptr: (System.IntPtr)adrBuffer);
        }
        WTSFreeMemory(ppBuffer: ppBuffer);
        return clientName;
    }

    private string GetTSClientDisplay(int sessionID)
    {
        System.IntPtr ppBuffer = System.IntPtr.Zero;
        uint pBytesReturned = 0;
        StringBuilder sDisplay = new StringBuilder();

        // Interface DLL avec les structures
        WTS_CLIENT_DISPLAY clientDisplay = new WTS_CLIENT_DISPLAY();
        Type dataType = typeof(WTS_CLIENT_DISPLAY);
        if (
            WTSQuerySessionInformation(
                hServer: System.IntPtr.Zero,
                sessionId: sessionID,
                wtsInfoClass: WTSInfoClass.WTSClientDisplay,
                ppBuffer: out ppBuffer,
                pBytesReturned: out pBytesReturned
            )
        )
        {
            clientDisplay = (WTS_CLIENT_DISPLAY)
                Marshal.PtrToStructure(ptr: ppBuffer, structureType: dataType);
            sDisplay.Append(
                value: "Display Horizontal: "
                    + (clientDisplay.HorizontalResolution).ToString()
                    + Environment.NewLine
            );
            sDisplay.Append(
                value: "Display Vertical: "
                    + (clientDisplay.VerticalResolution).ToString()
                    + Environment.NewLine
            );
            sDisplay.Append(value: "Display Color Depth: " + (clientDisplay.ColorDepth).ToString());
        }
        WTSFreeMemory(ppBuffer: ppBuffer);
        return sDisplay.ToString();
    }

    private string GetTSClientAddress(int sessionID)
    {
        System.IntPtr ppBuffer = System.IntPtr.Zero;
        uint pBytesReturned = 0;
        StringBuilder builder = new StringBuilder();
        // Interface avec API
        WTS_CLIENT_ADDRESS wtsAdr = new WTS_CLIENT_ADDRESS();

        if (
            WTSQuerySessionInformation(
                hServer: System.IntPtr.Zero,
                sessionId: sessionID,
                wtsInfoClass: WTSInfoClass.WTSClientAddress,
                ppBuffer: out ppBuffer,
                pBytesReturned: out pBytesReturned
            )
        )
        {
            // ---------------------------------------------------------------------------------------------
            // Pour pouvoir r�cup�rer l'ensembe des informations du client connect�
            // ---------------------------------------------------------------------------------------------
            wtsAdr.Address = new Byte[pBytesReturned - 1];
            int run = (int)ppBuffer; // pointeur sur les donn�es
            Type t = typeof(Byte);
            Type t1 = typeof(uint);
            int uintSize = Marshal.SizeOf(t: t1);
            int byteSize = Marshal.SizeOf(t: t);

            //-- Lecture du type d'@
            wtsAdr.AddressFamily = (uint)Marshal.ReadInt32(ptr: (System.IntPtr)run);

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
            for (int i = 0; i < pBytesReturned - 1; i++)
            {
                wtsAdr.Address[i] = Marshal.ReadByte(ptr: (System.IntPtr)run);
                run += byteSize;
                // TO GET and to SEE ALL the DATA
                //builder.Append(wtsAdr.Address[i].ToString()+"-");
            }
            //builder.Append("-");
            builder.Append(value: (wtsAdr.Address[4 + 2]).ToString());
            builder.Append(value: ".");
            builder.Append(value: (wtsAdr.Address[4 + 3]).ToString());
            builder.Append(value: ".");
            builder.Append(value: (wtsAdr.Address[4 + 4]).ToString());
            builder.Append(value: ".");
            builder.Append(value: (wtsAdr.Address[4 + 5]).ToString());
            // L'offset de 4 est du au fait que le type uint fait 4 Bytes et
            // le type de connexion est pris dans l'ensemble des donn�es
        }
        WTSFreeMemory(ppBuffer: ppBuffer);
        return builder.ToString();
    }

    void AddSection(string text, StringBuilder builder)
    {
        builder.Append(
            value: "===============================================" + Environment.NewLine
        );
        builder.AppendFormat(format: "====== {0}{1}", arg0: text, arg1: Environment.NewLine);
        builder.Append(
            value: "===============================================" + Environment.NewLine
        );
    }

    void AddHeader(string text, StringBuilder builder)
    {
        builder.Append(value: Environment.NewLine);
        builder.Append(value: text);
        builder.AppendFormat(
            format: "{0}{1}{2}",
            arg0: Environment.NewLine,
            arg1: "===============================================",
            arg2: Environment.NewLine
        );
    }

    void AddLine(string text, StringBuilder builder)
    {
        builder.AppendFormat(format: "{0}{1}", arg0: text, arg1: Environment.NewLine);
    }

    void AddInfo(string category, object data, StringBuilder builder)
    {
        string text;
        if (data == null)
        {
            text = "";
        }
        else
        {
            text = data.ToString();
        }
        builder.AppendFormat(
            format: "{0}: {1}{2}",
            arg0: category,
            arg1: text,
            arg2: Environment.NewLine
        );
    }

    private string[] GetWindowsIdentityRoles(WindowsIdentity identity)
    {
        object result = typeof(WindowsIdentity).InvokeMember(
            name: "_GetRoles",
            invokeAttr: BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
            binder: null,
            target: identity,
            args: new object[] { identity.Token },
            culture: null
        );
        return (string[])result;
    }

    private string MyXmlDateTime(DateTime date)
    {
        TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(time: date);
        int daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(year: date.Year).Delta.Hours;
        int hours = offset.Duration().Hours;
        int finalHours = hours; // + daylight;
        string sign = finalHours >= 0 ? "+" : "-";
        string result =
            date.ToString(format: "yyyy-MM-dd")
            + "T00:00:00.0000000"
            + sign
            + finalHours.ToString(format: "00")
            + ":"
            + offset.Minutes.ToString(format: "00");
        return result;
    }
    #endregion
}
