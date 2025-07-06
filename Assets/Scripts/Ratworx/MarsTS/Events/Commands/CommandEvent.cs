using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Receivers;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandEvent : AbstractEvent 
	{
		// public ICommandReceiver CommandReceiver { get; private set; }
		public ICommandable Unit { get; private set; }

		protected CommandEvent(
			string name,
			// ICommandReceiver commandReceiver,
			ICommandable unit
		) : base(
			"command" + name
		) {
			Unit = unit;
			// CommandReceiver = commandReceiver;
		}
	}
}