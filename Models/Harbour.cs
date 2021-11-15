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
                        List<Service> services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(Arg));
                        var containerService = new ContainerService();
                        containerService.Apply(services);
                    }
                    break;
                case Cmd.Add:
                    {
                        List<Service> services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(Arg));
                        var containerService = new ContainerService();
                        containerService.Add(services);
                    }
                    break;
                case Cmd.Remove:
                    {
                        var containerService = new ContainerService();
                        containerService.Remove(Arg);
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
                            List<Service> services = JsonConvert.DeserializeObject<List<Service>>(File.ReadAllText(Arg));
                            proxyService.Serve(services);
                        }
                    }
                    break;
            }
        }
    }
}