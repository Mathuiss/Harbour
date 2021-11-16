using System;
using System.Collections.Generic;
using System.IO;
using Harbour.Services;
using Newtonsoft.Json;

namespace Harbour.Models
{
    public class Harbour
    {
        public Cmd Cmd { get; set; }
        public string Arg { get; set; }
        public List<Service> Services { get; set; }

        public Harbour(Cmd cmd, string arg)
        {
            Cmd = cmd;
            Arg = arg;
        }

        public void Run()
        {
            switch (Cmd)
            {
                case Cmd.Apply:
                    {
                        FileCheck();
                        var containerService = new ContainerService();
                        containerService.Apply(Services);
                        File.WriteAllText(GetDefaultStatePath(), JsonConvert.SerializeObject(Services, Formatting.Indented));
                    }
                    break;
                case Cmd.Add:
                    {
                        List<Service> toAdd = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(Arg));
                        var containerService = new ContainerService();
                        containerService.Add(toAdd);

                        // Add to default config
                        Arg = null;
                        FileCheck();

                        foreach (Service service in toAdd)
                        {
                            if (!Services.Contains(service))
                            {
                                Services.Add(service);
                            }
                        }

                        File.WriteAllText(GetDefaultStatePath(), JsonConvert.SerializeObject(Services, Formatting.Indented));
                    }
                    break;
                case Cmd.Remove:
                    {
                        Services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(GetDefaultStatePath()));

                        if (Services.Exists(s => s.Name == Arg))
                        {
                            Services.Remove(Services.Find(s => s.Name == Arg));
                        }

                        var containerService = new ContainerService();
                        containerService.Apply(Services);
                        File.WriteAllText(GetDefaultStatePath(), JsonConvert.SerializeObject(Services, Formatting.Indented));
                    }
                    break;
                case Cmd.Serve:
                    {
                        var proxyService = new ProxyService();

                        if (Arg == "stop")
                        {
                            // Stop reverse proxy
                            proxyService.Stop();
                        }
                        else
                        {
                            // Start reverse proxy
                            FileCheck();
                            List<Service> services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(Arg));
                            proxyService.Serve(services);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// This check must be called before services are passed to a function.
        /// This check makes sure that there are always valid services.
        /// </summary>
        private void FileCheck()
        {
            // If no argument
            if (!string.IsNullOrEmpty(Arg))
            {
                Services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(Arg));
            }
            else
            {
                string filePath = GetDefaultStatePath();

                if (File.Exists(filePath))
                {
                    Services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(filePath));
                }
                else
                {
                    Services = new List<Service>();
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(Services, Formatting.Indented));
                }
            }
        }

        private string GetDefaultStatePath()
        {
            return Path.Combine(Environment.CurrentDirectory, "current-state.json");
        }
    }
}