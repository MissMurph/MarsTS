using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Commands {

    public interface ICommandable : IUnitInterface
	{
		event Action OnCommandsStateChanged;
		event Action OnCommandListChanged;
		Commandlet CurrentCommand { get; }
		int QueueCount { get; }
		List<ICommandReceiver> ActiveCommands { get; }
		List<Timer> Cooldowns { get; }
		void Order (Commandlet order, bool inclusive);
		CommandFactory EvaluateCommand (Entity target);
		Dictionary<string, ICommandReceiver> Commands ();
		bool CanCommand (string commandKey);
	}
}