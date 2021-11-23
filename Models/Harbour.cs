using System;
using System.Collections.Generic;
using System.IO;
using Harbour.Services;
using Newtonsoft.Json;

namespace Harbour.Models
{
    public class Harbour
    {
        public List<Service> Services { get; set; }

        public Harbour()
        {
        }

        public void Apply(string path)
        {
            FileCheck(path);
            var containerService = new ContainerService();
            containerService.Apply(Services);
            File.WriteAllText(GetDefaultStatePath(), JsonConvert.SerializeObject(Services, Formatting.Indented));
        }

        public void Add(string path)
        {
            List<Service> toAdd = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(path));
            var containerService = new ContainerService();
            containerService.Add(toAdd);

            foreach (Service service in toAdd)
            {
                if (!Services.Contains(service))
                {
                    Services.Add(service);
                }
            }

            File.WriteAllText(GetDefaultStatePath(), JsonConvert.SerializeObject(Services, Formatting.Indented));
        }

        public void Remove(string serviceName)
        {
            Services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(GetDefaultStatePath()));

            if (Services.Exists(s => s.Name == serviceName))
            {
                Services.Remove(Services.Find(s => s.Name == serviceName));
            }
            else
            {
                throw new ArgumentException($"Service was not found in running configuration: {serviceName}");
            }

            var containerService = new ContainerService();
            containerService.Apply(Services);
            File.WriteAllText(GetDefaultStatePath(), JsonConvert.SerializeObject(Services, Formatting.Indented));
        }

        public void Serve(string path)
        {
            FileCheck(path);

            var proxyService = new ProxyService(Services);
            proxyService.StartServer();
        }

        public void ServeBackground(string path)
        {
            FileCheck(path);

            var proxyService = new ProxyService(Services);
            proxyService.StartServerBackground();
        }

        public void StopServe()
        {
            var proxyService = new ProxyService();
            proxyService.Stop();
        }

        /// <summary>
        /// This check must be called before services are passed to a function.
        /// This check makes sure that there are always valid services.
        /// </summary>
        private void FileCheck(string path)
        {
            // If no argument
            if (!string.IsNullOrEmpty(path))
            {
                Services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(path));
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