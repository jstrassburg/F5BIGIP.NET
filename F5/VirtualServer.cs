using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace F5
{
	/// <summary>
	/// Represents a host virtual server.
	/// </summary>
	/// <remarks>To support network virtual servers this class will be extended to allow specification
	/// of a netmask (in addition to address) which would hit the wildmasks parameter of the create
	/// method on the iControl LocalLBVirtualServer create method.</remarks>
	public class VirtualServer
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public string Address { get; set; }

		/// <summary>
		/// The service port for this virtual server or zero (0) for all ports
		/// </summary>
		public long Port { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public VirtualServerProtocol VirtualServerProtocol { get; set; }

		public string DefaultPoolName { get; set; }

		public IEnumerable<VirtualServerProfile> Profiles { get; set; }
	}
}
