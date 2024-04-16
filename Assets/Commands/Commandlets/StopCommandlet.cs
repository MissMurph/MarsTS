using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

	public class StopCommandlet : Commandlet {
		public override Type TargetType { get { return typeof(bool); } }

		public override string Key => Name;

		public override Commandlet Clone () {
			throw new NotImplementedException();
		}
	}
}