using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Receivers;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandWorkEvent : CommandEvent 
	{
		/// <summary>The progress of work done, between 0 - 1.</summary>
		public float Progress { get; private set; }
		public Commandlet Command { get; private set; }

		public CommandWorkEvent (
			Commandlet command,
			ICommandable unit,
			float progress
		) : base(
			"Work",
			unit
		) {
			Progress = progress;
			Command = command;
		}
	}
}