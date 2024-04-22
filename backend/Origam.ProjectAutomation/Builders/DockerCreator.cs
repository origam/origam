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

using Docker.DotNet.Models;
using Origam.Docker;
using Origam.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Origam.ProjectAutomation.Builders;

public class DockerCreator : AbstractBuilder
{
    public override string Name => "Start Docker Container";

    private bool IsNewVolume { get; set; } = false;
    public string ContainerID { get; private set; }

    private readonly DockerManager dockerManager;

    public DockerCreator(string tag,string dockerApiAdress)
    {
            this.dockerManager = new DockerManager(tag, dockerApiAdress);
        }

    public override void Execute(Project project)
    {
            Task<Docker.OperatingSystem> task = Task.Run(() =>
            {
                return new Docker.OperatingSystem(dockerManager.IsDockerInstaledAsync());
            }
            );
            Docker.OperatingSystem dockerOs = task.Result;

            if (dockerOs.IsOnline())
            {
                throw new Exception("Docker is prerequired. Please install Docker.");
            }
            if(DockerVolumeExists(project.Name))
            {
                throw new Exception(string.Format("Docker Volume {0} already exists. " +
                    "Please chose different Project Name.",project.Name));
            }
            else
            {
                IsNewVolume = true;
            }
            dockerManager.CreateVolume(project.Name);
            string containerId = StartDockerContainer(project.DatabasePassword, project);
            if(!IsContainerPrepared(containerId))
            { 
                throw new Exception(string.Format("Docker didn't start. " +
                    "Please check the container {0} logs.",project.Name));
            }
        }

    private bool DockerVolumeExists(string volumeName)
    {
            Task<VolumesListResponse> task = Task.Run(() =>
            {
                return dockerManager.DockerVolumeExistsAsync();
            }
            );
            var vol = task.ConfigureAwait(false).GetAwaiter().GetResult();
            return vol.Volumes.Where(volumelist => volumelist.Name == volumeName).Any();
        }

    private bool IsContainerPrepared(string containerId)
    {
            long dockerdateTime = DateTime.Now.AddSeconds(60).Ticks;
            while (DateTime.Now.Ticks < dockerdateTime)
            {
                Thread.Sleep(5000);
                if (IsContainerRunningProperly(containerId))
                {
                    return true;
                }
            }
            return false;
        }
    private string StartDockerContainer(string databaseAdminPassword, Project project)
    {
            DockerContainerParameter containerParameters = new DockerContainerParameter
            {
                DockerEnvPath = project.DockerEnvPath,
                AdminPassword = databaseAdminPassword,
                ProjectName = project.Name,
                SourceFolder = project.SourcesFolder,
                DockerPort = project.DockerPort.ToString(),
                DockerSourcePath = project.DockerSourcePath
            };
            Task<CreateContainerResponse> task = Task.Run(() =>
                {
                    return dockerManager.StartDockerContainerAsync(containerParameters);
                }
            );

            ContainerID = task.
                ConfigureAwait(false).
                GetAwaiter().
                GetResult().
                ID;
            return ContainerID;
        }
    private bool IsContainerRunningProperly(string containerId)
    {

            string output = GetDockerLogsAsync(containerId);
            if (output.Contains("Press [CTRL+C] to stop"))
            {
                return true;
            }
            if (output.Contains("OrigamServer.dll"))
            {
                throw new Exception("Docker started with an error. Please check Docker logs.");
            }
            return false;
        }

    private string GetDockerLogsAsync(string containerId)
    {
            string output;
            Task<Stream> task = Task.Run(() =>
            {
                return dockerManager.GetDockerLogsAsync(containerId);
            }
            );
            var streamlogs = task.
                ConfigureAwait(false).
                GetAwaiter().
                GetResult();
            using (var reader = new StreamReader(streamlogs))
            {
                output = reader.ReadToEnd();
            }
            return output;
        }

    public override void Rollback()
    {
           if(IsNewVolume)
            {
                Task.Run(() =>
                {
                    _ = dockerManager.RemoveContainerAsync(ContainerID);
                }
                ). ConfigureAwait(false).
                    GetAwaiter().
                    GetResult();
            }
        }
}