using System;
using System.Collections.Generic;

namespace Harbour.Models
{
    public class Service : IEquatable<Service>
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Endpoint { get; set; }
        public List<Container> Containers { get; set; }

        public Service()
        {
            Containers = new List<Container>();
        }

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

        public bool Equals(Service other)
        {
            if (this.Name != other.Name)
                return false;

            if (this.Domain != other.Domain)
                return false;

            if (this.Endpoint != other.Endpoint)
                return false;

            foreach (Container container in this.Containers)
            {
                if (!other.Containers.Contains(container))
                    return false;
            }

            return true;
        }
    }
}