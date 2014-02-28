using System;
using System.Runtime.Serialization;

namespace F5
{
	[Serializable]
	public class F5Exception : Exception
	{
		public F5Exception(Exception exception) : base(exception.Message, exception)
		{
		}
	}
}
