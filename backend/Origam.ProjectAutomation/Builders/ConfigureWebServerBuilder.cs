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
using System.IO;
using System.Security.AccessControl;
using Microsoft.Web.Administration;

namespace Origam.ProjectAutomation;

public class ConfigureWebServerBuilder : AbstractBuilder
{
    ServerManager _serverManager;
    Site _site;
    Application _application;
    ApplicationPool _pool;

    public override string Name
    {
        get
        {
                return "Configure Web Server";
            }
    }

    public override void Execute(Project project)
    {
            string applicationPoolName = project.Name;
            _serverManager = new ServerManager();
            _site = _serverManager.Sites[project.WebRootName];
            _application = _site.Applications.Add("/" + project.Url, project.BinFolder);
            _application.ApplicationPoolName = applicationPoolName;
            Configuration config = _serverManager.GetApplicationHostConfiguration();
            SetAuthentication(config, project.WebRootName, "anonymousAuthentication", true);
            SetAuthentication(config, project.WebRootName, "basicAuthentication", false);
            SetAuthentication(config, project.WebRootName, "windowsAuthentication", false);
            _pool = _serverManager.ApplicationPools.Add(applicationPoolName);
            _pool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
            _pool.ManagedRuntimeVersion = "v4.0";
            // LoadUserProfile must be turned on so self-registration works
            _pool.ProcessModel.LoadUserProfile = true;
            _pool.Recycling.PeriodicRestart.Time = new System.TimeSpan(0);
            _pool.ProcessModel.IdentityType = ProcessModelIdentityType.ApplicationPoolIdentity;
            _serverManager.CommitChanges();

            System.Threading.Thread.Sleep(800);
            AddPermissionsToSourceFolder(project);
        }

    private void AddPermissionsToSourceFolder(Project project)
    {
            string accountName = @"IIS APPPOOL\" + project.Name;
            DirectoryInfo sourceDirInfo = new DirectoryInfo(project.ModelSourceFolder);

            DirectorySecurity dSecurity = sourceDirInfo.GetAccessControl();

            dSecurity.AddAccessRule(new FileSystemAccessRule(
                accountName,
                FileSystemRights.CreateFiles | FileSystemRights.DeleteSubdirectoriesAndFiles,
                InheritanceFlags.None,
                PropagationFlags.None,
                AccessControlType.Allow));
            dSecurity.AddAccessRule(new FileSystemAccessRule(
                accountName,
                FileSystemRights.Read,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            sourceDirInfo.SetAccessControl(dSecurity);
        }

    private static void SetAuthentication(Configuration config, string name, string authenticationType, bool enabled)
    {
            ConfigurationSection authenticationSection =
                config.GetSection("system.webServer/security/authentication/" + authenticationType,
                name);
            authenticationSection["enabled"] = enabled;
        }

    public override void Rollback()
    {
            ServerManager serverManager = new ServerManager();
            RollbackApplication(serverManager);
            RollbackPool(serverManager);
            serverManager.CommitChanges();
        }

    public string[] WebSites()
    {
            ServerManager serverManager = new ServerManager();
            string[] result = new string[serverManager.Sites.Count];
            for (int i = 0; i < serverManager.Sites.Count; i++)
            {
                Site site = serverManager.Sites[i];
                result[i] = site.Name;
            }
            return result;
        }

    public string WebSiteUrl(string webSiteName)
    {
            ServerManager serverManager = new ServerManager();
            Site site = serverManager.Sites[webSiteName];
            if (site == null)
            {
                throw new Exception("The web site: "+ webSiteName+ " was not found. Make sure you have entered name of an existing web site.");
            }
            if (site.Bindings.Count > 0)
            {
                string host;
                Binding binding = site.Bindings[0];
                string[] bindingInfo = binding.BindingInformation.Split(":".ToCharArray());
                if (!string.IsNullOrEmpty(bindingInfo[2]))
                {
                    // host name is set
                    host = bindingInfo[2];
                }
                else if (bindingInfo[0] == "*")
                {
                    // all ip addresses, we will use localhost
                    host = "localhost";
                }
                else
                {
                    // an ip address is set
                    host = bindingInfo[0];
                }
                string port = "";
                if (bindingInfo[1] != "80")
                {
                    port = ":" + bindingInfo[1];
                }
                return binding.Protocol + "://" + host + port;
            }
            return null;
        }

    private void RollbackPool(ServerManager serverManager)
    {
            ApplicationPool pool = serverManager.ApplicationPools[_pool.Name];
            serverManager.ApplicationPools.Remove(pool);
        }

    private void RollbackApplication(ServerManager serverManager)
    {
            Site site = serverManager.Sites[_site.Name];
            Application application = site.Applications[_application.Path];
            site.Applications.Remove(application);
        }
}