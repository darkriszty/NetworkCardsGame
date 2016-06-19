using System.Diagnostics;

namespace Shared.Diagnostics
{
	public class TraceSourceFactory
	{
		public static TraceSource GetDefaultTraceSource()
		{
			return new TraceSource("dev");
		}
	}
}
