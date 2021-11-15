using System.Collections.Generic;

namespace Harbour.Models
{
    public class Service
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Endpoint { get; set; }
        public List<Container> Containers { get; set; }

        public Service(string name, List<Container> containers)
        {
            Name = name;
            Containers = containers;
        }

        public Service(string name, string endpoint, List<Container> containers)
        {
            Name = name;
            Endpoint = endpoint;
            Containers = containers;
        }

        public Service(string name, string domain, string endpoint, List<Container> containers)
        {
            Name = name;
            Domain = domain;
            Endpoint = endpoint;
            Containers = containers;
        }
    }
}