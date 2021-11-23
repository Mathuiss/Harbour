using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Harbour.Models
{
    public class Container : IEquatable<Container>
    {
        public string Name { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Image { get; set; }
        public string Endpoint { get; set; }
        public string HttpPort { get; set; }
        public string Restart { get; set; }
        public string[] Ports { get; set; }
        public string[] Volumes { get; set; }
        public string[] Env { get; set; }

        public Container()
        {
            Ports = new string[0];
            Volumes = new string[0];
            Env = new string[0];
        }

        public bool Equals(Container other)
        {
            if (this.Name != other.Name)
                return false;

            if (this.Image != other.Image)
                return false;

            if (this.Restart != other.Restart)
                return false;

            if (!Enumerable.SequenceEqual(this.Ports, other.Ports))
                return false;

            if (!Enumerable.SequenceEqual(this.Volumes, other.Volumes))
                return false;

            if (!Enumerable.SequenceEqual(this.Env, other.Env))
                return false;

            return true;
        }
    }
}