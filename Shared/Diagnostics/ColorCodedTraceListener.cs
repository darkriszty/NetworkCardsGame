using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Shared.Diagnostics
{
	public class ColorCodedTraceListener : ConsoleTraceListener
	{
		private static Dictionary<TraceEventType, ConsoleColor> _colors = new Dictionary<TraceEventType, ConsoleColor>
		{
			{ TraceEventType.Verbose, ConsoleColor.DarkGray },
			{ TraceEventType.Information, ConsoleColor.White },
			{ TraceEventType.Warning, ConsoleColor.Yellow },
			{ TraceEventType.Error, ConsoleColor.Red },
			{ TraceEventType.Critical, ConsoleColor.Red },
		};

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			ConsoleColor originalColor = Console.ForegroundColor;
			ConsoleColor color;
			if (_colors.TryGetValue(eventType, out color))
			{
				Console.ForegroundColor = color;
			}

			base.TraceEvent(eventCache, source, eventType, id, format, args);
			Console.ForegroundColor = originalColor;
		}
	}
}
