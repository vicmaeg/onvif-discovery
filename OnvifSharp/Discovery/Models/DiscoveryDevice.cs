using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OnvifSharp.Discovery.Models
{
    public class DiscoveryDevice
    {
        public IEnumerable<string> Types { get; internal set; }
        public IEnumerable<string> Scopes { get; internal set; }
        public IEnumerable<string> XAdresses { get; internal set; }
        public string MetadataVersion { get; internal set; }
        public IPAddress Address { get; internal set; }
    }
}
