using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Harbour.Models;
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
            Console.WriteLine("Applying service configuration...");

            var newContainers = new List<Container>();
            services.ForEach(s => newContainers.AddRange(s.Containers));

            foreach (Container container in RunningContainers)
            {
                if (!newContainers.Contains(container))
                {
                    RemoveContainer(container.Name);
                    RunningContainers.Remove(container);
                }
            }

            foreach (Container container in newContainers)
            {
                if (!RunningContainers.Contains(container))
                {
                    // Container does not yet exist, so run
                    RunContainer(container);
                }
            }


            Console.WriteLine("Success");
        }

        /// <summary>
        /// Adds one or more services to the configuration.
        /// If a service already exists, it is ignored.
        /// </summary>
        /// <param name="services"></param>
        public void Add(List<Service> services)
        {
            Console.WriteLine("Adding service configuration...");

            foreach (Service service in services)
            {
                foreach (Container container in service.Containers)
                {
                    RunContainer(container);
                }
            }

            Console.WriteLine("Success");
        }

        /// <summary>
        /// Removes a service from configuration.
        /// If a service does not exist, does nothing.
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            Console.WriteLine("Removing service configuration...");

            if (!RunningContainers.Exists(c => c.Name == name))
            {
                throw new ArgumentException($"Invalid argument. Container {name} does not exist.");
            }

            RemoveContainer(name);

            Console.WriteLine("Success");
        }

        private void RunContainer(Container container)
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

            if (container.Ports != null)
            {
                foreach (string envVar in container.EnvironmentVars)
                {
                    args = string.Concat(args, $" -e {envVar}");
                }
            }

            args = string.Concat(args, $" {container.Image}");

            Process.Start("docker", args);
        }

        private void RemoveContainer(string name)
        {
            Process.Start("docker", $"rm -f {name}");
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

            using (var proc = Process.Start("docker", "ps -aq"))
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
            using (var proc = Process.Start("docker", $"inspect {id}"))
            {
                using (var reader = proc.StandardOutput)
                {
                    string json = reader.ReadToEnd();
                    JObject info = JObject.Parse(json);

                    // Get name
                    string name = (string)info[0]["Name"];

                    if (name.StartsWith('/'))
                        name = name.Remove(0); // Strip beginning / from name

                    // Get image
                    string image = (string)info[0]["Config"]["Image"];

                    // Get restart policy
                    string restart = (string)info[0]["HostConfig"]["RestartPolicy"]["Name"];

                    // Get ports
                    var ports = new List<string>();
                    JToken portinfo = info[0]["HostConfig"]["PortBindings"];
                    foreach (JProperty prop in portinfo)
                    {
                        string containerPort = prop.Name.Split('/')[0];
                        string hostPort = (string)info[0]["HostConfig"]["PortBindings"][prop.Name]["HostPort"];
                        ports.Add($"{hostPort}:{containerPort}");
                    }

                    // Get volumes
                    JArray binds = (JArray)info[0]["HostConfig"]["Binds"];
                    var volumes = new List<string>();
                    foreach (string vol in binds.Values())
                    {
                        volumes.Add(vol);
                    }

                    // Get environment variables
                    JArray envVars = (JArray)info[0]["Config"]["Env"];
                    var environmentVars = new List<string>();
                    foreach (string var in envVars.Values())
                    {
                        if (!var.Contains("PATH"))
                        {
                            environmentVars.Add(var);
                        }
                    }

                    return new Container()
                    {
                        Name = name,
                        Image = image,
                        Restart = restart,
                        Ports = ports.ToArray(),
                        Volumes = volumes.ToArray(),
                        EnvironmentVars = environmentVars.ToArray(),
                    };
                }
            }
        }
    }
}