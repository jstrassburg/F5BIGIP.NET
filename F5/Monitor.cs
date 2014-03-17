
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace F5
{
	/// <summary>
	/// Represents a monitor on the BIGIP
	/// </summary>
	public class Monitor
	{
		public string Name { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public MonitorType MonitorType { get; set; }
	}
}
