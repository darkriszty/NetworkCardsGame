using System.Diagnostics;

namespace Shared.Diagnostics
{
	public static class TraceSourceExtensions
	{
		private const int DEFAULT_VERBOSE = 3;
		private const int DEFAULT_WARNING = 1;
		private const int DEFAULT_ERROR = 0;

		public static void TraceVerbose(this TraceSource traceSource, string format, params object[] args)
		{
			TraceVerbose(traceSource, DEFAULT_VERBOSE, format, args);
		}

		public static void TraceVerbose(this TraceSource traceSource, int eventNumber, string format, params object[] args)
		{
			traceSource.TraceEvent(TraceEventType.Verbose, eventNumber, format, args);
		}

		public static void TraceWarning(this TraceSource traceSource, string format, params object[] args)
		{
			TraceWarning(traceSource, DEFAULT_WARNING, format, args);
		}

		public static void TraceWarning(this TraceSource traceSource, int eventNumber, string format, params object[] args)
		{
			traceSource.TraceEvent(TraceEventType.Warning, eventNumber, format, args);
		}

		public static void TraceError(this TraceSource traceSource, string format, params object[] args)
		{
			TraceError(traceSource, DEFAULT_ERROR, format, args);
		}

		public static void TraceError(this TraceSource traceSource, int eventNumber, string format, params object[] args)
		{
			traceSource.TraceEvent(TraceEventType.Warning, eventNumber, format, args);
		}
	}
}
