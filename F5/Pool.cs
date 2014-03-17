using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace F5
{
	/// <summary>
	/// Represents a pool on the BIGIP
	/// </summary>
	public class Pool
	{
		public string Name { get; set; }


		[JsonConverter(typeof(StringEnumConverter))]
		public LoadBalancingMethod LoadBalancingMethod { get; set; }

		public IEnumerable<PoolMember> Members { get; set; }

		public IEnumerable<string> Monitors { get; set; }
	}
}
