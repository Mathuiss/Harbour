using System;
using System.Collections.Generic;

namespace Harbour.Models
{
    public class Service : IEquatable<Service>
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public List<Container> Containers { get; set; }

        public Service()
        {
            Containers = new List<Container>();
        }

        public bool Equals(Service other)
        {
            if (this.Name != other.Name)
                return false;

            if (this.Domain != other.Domain)
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