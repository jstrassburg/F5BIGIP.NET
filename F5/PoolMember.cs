namespace F5
{
	/// <summary>
	/// Represents a pool member on the BIGIP
	/// </summary>
	public class PoolMember
	{
		public string Address { get; set; }
		public long Port { get; set; }
	}
}