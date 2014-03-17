namespace F5
{
	/// <summary>
	/// Represents a node on the BIGIP
	/// </summary>
	public class Node
	{
		public string Name { get; set; }
		public string Address { get; set; }
		public long ConnectionLimit { get; set; }
	}
}
