using System;

namespace F5
{
	[Serializable]
	public class F5Exception : Exception
	{
		public F5Exception(string message)
			: base(message)
		{
		}

		public F5Exception(Exception exception)
			: base(exception.Message, exception)
		{
		}
	}
}
