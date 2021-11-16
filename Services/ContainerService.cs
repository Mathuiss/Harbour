using System;
using System.Collections.Generic;
using System.Diagnostics;
using Harbour.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Harbour.Services
{
    public class ContainerService
    {
        public List<Container> RunningContainers { get; set; }

        public ContainerService()
        {
            RunningContainers = GetRunningConfig();
        }

        /// <summary>
        /// Applies given configuration to the system.
        /// Finds differences between running and applying config.
        /// Removes old containers and adds new ones.
        /// </summary>
        /// <param name="services"></param>
        public void Apply(List<Service> services)
        {
            var applyContainers = new List<Container>();
            services.ForEach(s => s.Containers.ForEach(c => applyContainers.Add(c)));

            foreach (Container runningContainer in RunningContainers.GetRange(0, RunningContainers.Count))
            {
                if (!applyContainers.Contains(runningContainer))
                {
                    Process p = RemoveContainer(runningContainer.Name);
                    p.WaitForExit();
                    p.Close();
                    RunningContainers.Remove(runningContainer);
                }
            }

            var processes = new List<Process>();

            foreach (Container container in applyContainers)
            {
                if (!RunningContainers.Contains(container))
                {
                    // Container does not yet exist, so run
                    processes.Add(RunContainer(container));
                }
            }

            foreach (Process p in processes)
            {
                p.WaitForExit();
                p.Close();
            }
        }

        /// <summary>
        /// Adds one or more services to the configuration.
        /// If a service already exists, it is ignored.
        /// </summary>
        /// <param name="services"></param>
        public void Add(List<Service> services)
        {
            var processes = new List<Process>();

            foreach (Service service in services)
            {
                foreach (Container container in service.Containers)
                {
                    if (RunningContainers.Contains(container))
                    {
                        Console.WriteLine($"Container already exists and is ignored: {container.Name}");
                    }
                    else
                    {
                        processes.Add(RunContainer(container));
                    }
                }
            }

            foreach (Process p in processes)
            {
                p.WaitForExit();
                p.Close();
            }
        }

        private Process RunContainer(Container container)
        {
            string args = "run -d";

            if (!string.IsNullOrEmpty(container.Name))
            {
                args = string.Concat(args, $" --name {container.Name}");
            }

            if (!string.IsNullOrEmpty(container.Restart))
            {
                args = string.Concat(args, $" --restart {container.Restart}");
            }

            if (container.Ports != null)
            {
                foreach (string port in container.Ports)
                {
                    args = string.Concat(args, $" -p {port}");
                }
            }

            if (container.Volumes != null)
            {
                foreach (string volume in container.Volumes)
                {
                    args = string.Concat(args, $" -v {volume}");
                }
            }

            if (container.Env != null)
            {
                foreach (string envVar in container.Env)
                {
                    args = string.Concat(args, $" -e {envVar}");
                }
            }

            args = string.Concat(args, $" {container.Image}");

            var startInfo = new ProcessStartInfo("docker", args);
            startInfo.RedirectStandardOutput = false;
            return Process.Start(startInfo);
        }

        private Process RemoveContainer(string name)
        {
            var startInfo = new ProcessStartInfo("docker", $"rm -f {name}");
            startInfo.RedirectStandardOutput = false;

            return Process.Start(startInfo);
        }

        private List<Container> GetRunningConfig()
        {
            var containers = new List<Container>();
            List<string> containerIds = GetContainerIds();

            foreach (string containerId in containerIds)
            {
                containers.Add(GetContainerInfo(containerId));
            }

            return containers;
        }

        private List<string> GetContainerIds()
        {
            var containerIds = new List<string>();

            var startInfo = new ProcessStartInfo("docker", "ps -aq");
            startInfo.RedirectStandardOutput = true;

            using (var proc = Process.Start(startInfo))
            {
                using (var r = proc.StandardOutput)
                {
                    while (!r.EndOfStream)
                    {
                        containerIds.Add(r.ReadLine());
                    }
                }
            }

            return containerIds;
        }

        private Container GetContainerInfo(string id)
        {
            var startInfo = new ProcessStartInfo("docker", $"inspect {id}");
            startInfo.RedirectStandardOutput = true;

            using (var proc = Process.Start(startInfo))
            {
                using (var reader = proc.StandardOutput)
                {
                    string json = reader.ReadToEnd();
                    JArray info = JArray.Parse(json);

                    // Get name
                    string name = (string)info[0]["Name"];

                    if (name.StartsWith('/'))
                        name = name.TrimStart('/'); // Strip beginning / from name

                    // Get image
                    string image = (string)info[0]["Config"]["Image"];

                    // Get restart policy
                    string restart = (string)info[0]["HostConfig"]["RestartPolicy"]["Name"];

                    if (restart.Equals("no"))
                    {
                        restart = null;
                    }

                    // Get ports
                    var ports = new List<string>();
                    JToken portinfo = info[0]["HostConfig"]["PortBindings"];

                    if (portinfo != null)
                    {
                        foreach (JProperty prop in portinfo)
                        {
                            string hostPort = (string)info[0]["HostConfig"]["PortBindings"][prop.Name][0]["HostPort"];
                            string containerPort = prop.Name.Split('/')[0];
                            ports.Add($"{hostPort}:{containerPort}");
                        }
                    }

                    // Get volumes
                    var volumes = new List<string>();
                    JToken binds = info[0]["HostConfig"]["Binds"];

                    if (binds != null)
                    {
                        foreach (string vol in binds.Values())
                        {
                            volumes.Add(vol);
                        }
                    }

                    // Get environment variables
                    var environmentVars = new List<string>();
                    JToken envVars = info[0]["Config"]["Env"];

                    if (envVars != null)
                    {
                        foreach (string var in envVars.Values())
                        {
                            if (!var.Contains("PATH"))
                            {
                                environmentVars.Add(var);
                            }
                        }
                    }

                    return new Container()
                    {
                        Name = name,
                        Image = image,
                        Restart = restart,
                        Ports = ports.ToArray(),
                        Volumes = volumes.ToArray(),
                        Env = environmentVars.ToArray(),
                    };
                }
            }
        }
    }
}