
namespace F5
{
	/// <summary>
	/// These are taken from the iControl enum LocalLBLBMethod
	/// </summary>
	public enum LoadBalancingMethod
	{
		RoundRobin = 0,
		RatioMember = 1,
		LeastConnectionMember = 2,
		ObservedMember = 3,
		PredictiveMember = 4,
		RatioNodeAddress = 5,
		LeastConnectionNodeAddress = 6,
		FastestNodeAddress = 7,
		ObservedNodeAddress = 8,
		PredictiveNodeAddress = 9,
		DynamicRatio = 10,
		FastestAppResponse = 11,
		LeastSessions = 12,
		DynamicRatioMember = 13,
		L3Addr = 14,
		Unknown = 15,
		WeightedLeastConnectionMember = 16,
		WeightedLeastConnectionNodeAddress = 17,
		RatioSession = 18,
		RatioLeastConnectionMember = 19,
		RatioLeastConnectionNodeAddress = 20,
	}
}
