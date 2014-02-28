
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace F5
{
	public class Monitor
	{
		public string Name { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public MonitorType MonitorType { get; set; }
	}
}
