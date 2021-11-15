using Newtonsoft.Json;

namespace Harbour.Models
{
    public class Container
    {
        public string Name { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Image { get; set; }
        public string Restart { get; set; }
        public string[] Ports { get; set; }
        public string[] Volumes { get; set; }
        public string[] EnvironmentVars { get; set; }
    }
}