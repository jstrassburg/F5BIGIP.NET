
namespace F5
{
	/// <summary>
	/// These are taken from the iControl enum CommonProtocolType
	/// and are only the types that are usable with a virtual server
	/// </summary>
	public enum VirtualServerProtocol
	{
		TransmissionControlProtocol = 6,
		UserDatagramProtocol = 7,
		StreamControlTransmissionProtocol = 11,
	}
}
