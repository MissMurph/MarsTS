using System.Collections.Generic;
using Ratworx.MarsTS.Production;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories {

	public class Upgrade : Produce {
		public override string Name => "upgrade";

		public override Sprite Icon => icon;

		public override string Description => _description;
	}
}