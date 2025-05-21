using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandWorkEvent : CommandEvent 
	{
		/// <summary>The progress of work done, between 0 - 1.</summary>
		public float Progress { get; private set; }

		public CommandWorkEvent (
			Commandlet command,
			ICommandable unit,
			float progress
		) : base(
			"Work",
			command,
			unit
		) {
			Progress = progress;
		}
	}
}