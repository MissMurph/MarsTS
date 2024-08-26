using System;

namespace MarsTS.Commands {

	public class StopCommandlet : Commandlet<bool> {

		public override string Key => Name;

		public override Commandlet Clone () {
			throw new NotImplementedException();
		}
	}
}