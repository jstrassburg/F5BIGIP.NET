using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace F5
{
	/// <summary>
	/// Definition inspired by iControl's LocalLBVirtualServerVirtualServerProfile
	/// </summary>
	public class VirtualServerProfile
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public VirtualServerProfileContext VirtualServerProfileContext { get; set; }

		public string Name { get; set; }
	}
}
