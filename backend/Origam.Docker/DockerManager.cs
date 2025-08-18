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

using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Origam.Docker;
public class DockerManager
{
    private readonly DockerClient client;
    private readonly string prefix = "";
    private readonly string tag;
    private readonly string repository = "origam/server";
    private string Imagename
    {
        get
        {
            //"origam/server:pg_master-latest"
            return string.Format("{0}:{1}{2}",repository,prefix,tag);
        }
    }
    public DockerManager(string tag,string dockeradress)
    {
            client = new DockerClientConfiguration(
                new Uri(dockeradress)).CreateClient();
        this.tag = tag;
    }
    public async Task<VersionResponse> IsDockerInstaledAsync()
    {
        return await client.System.GetVersionAsync();
    }
    public bool CreateVolume(string name)
    {
        VolumesCreateParameters volumesCreateParameters = new VolumesCreateParameters
        {
            Name = name
        };
        try
        {
            var task = Task.Run(async () =>
            {
                return await client.Volumes.CreateAsync(volumesCreateParameters);
            }).GetAwaiter().GetResult();
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }
    public async Task<VolumesListResponse> DockerVolumeExistsAsync()
    {
           return await client.Volumes.ListAsync();
    }
    public bool PullImage()
    { 
        var progress = new Progress<JSONMessage>();
        client.Images.CreateImageAsync(
                new ImagesCreateParameters()
                {
                    FromImage = repository,
                    Tag = prefix + tag
                }, null,
                progress);
        return true;
    }
    public async Task RemoveContainerAsync(string containerID)
    {
                await client.Containers.RemoveContainerAsync(containerID,
                    new ContainerRemoveParameters 
                    { 
                        RemoveVolumes = true,
                        Force = true
                    });
    }
    public async Task<Stream> GetDockerLogsAsync(string id)
    {
        ContainerLogsParameters logparams = new ContainerLogsParameters { ShowStdout = true };
        return await client.Containers.GetContainerLogsAsync(id, false, logparams, CancellationToken.None);
    }
    public async Task<CreateContainerResponse> StartDockerContainerAsync(DockerContainerParameter containerParameter)
    {
        if (!IsImageAlreadyPulled())
        {
            throw new Exception(
                string.Format("Docker image {0} didnt pull in time. Please check log.", 
                repository));
        }
        IList<string> env = new List<string>();
        string[] envfile = File.ReadAllLines(containerParameter.DockerEnvPath);
        env.Add(string.Format("PG_Origam_Password={0}", containerParameter.AdminPassword));
        foreach (string line in envfile)
        {
            env.Add(line);
        }
        IList<Mount> mounts = new List<Mount>
        {
            new Mount { Source = containerParameter.ProjectName, Target = "/var/lib/postgresql",Type = "volume"},
        };
        if(!string.IsNullOrEmpty(containerParameter.DockerSourcePath))
        {
            mounts.Add(new Mount { Source = containerParameter.DockerSourcePath, Target = "/home/origam/HTML5/data/origam", Type = "bind" });
        }
        else
        {
            mounts.Add(new Mount { Source = containerParameter.SourceFolder, Target = "/home/origam/HTML5/data/origam", Type = "bind" });
        }
        IDictionary<string, IList<PortBinding>> portbind = new Dictionary<string, IList<PortBinding>>
        {
            { "8080/tcp", new List<PortBinding> { new PortBinding {HostPort = containerParameter.DockerPort } }},
            { "5433/tcp", new List<PortBinding> { new PortBinding {HostPort = "5433" }}}
        };
        HostConfig hostconfig = new HostConfig
        {
            Mounts = mounts,
            PortBindings = portbind,
            PublishAllPorts = true
        };
        IDictionary<string, EmptyStruct> exposePort = new Dictionary<string, EmptyStruct>
        {
            { "5433/tcp", default },
            { "8080/tcp", default }
        };
        CreateContainerParameters create_containerParameters = new CreateContainerParameters
        {
            Name = containerParameter.ProjectName,
            Image = Imagename,
            Env = env,
            HostConfig = hostconfig,
            ExposedPorts = exposePort
        };
        var containerTask =  await client.Containers.CreateContainerAsync(
                                create_containerParameters);
        var createcontainerID = containerTask.ID;
        await client.Containers.StartContainerAsync(id: createcontainerID,
                                new ContainerStartParameters());
        return containerTask;
    }
    private  bool IsImageAlreadyPulled()
    {
        long dockerdateTime = DateTime.Now.AddSeconds(60).Ticks;
        while (DateTime.Now.Ticks < dockerdateTime)
        {
            Thread.Sleep(5000);
            if (DoesImageExist())
            {
                return true;
            }
        }
        return false;
    }
    private bool DoesImageExist()
    {
        try
        {
            var inspectImageTask = Task.Run(async () =>
            {
                return await client.Images.InspectImageAsync(Imagename);
            }).GetAwaiter().GetResult();
        } catch
        {
            return false;
        }
        return true;
    }
}
