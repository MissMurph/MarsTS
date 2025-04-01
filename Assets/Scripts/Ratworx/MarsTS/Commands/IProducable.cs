using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ratworx.MarsTS.Commands {

	public interface IProducable {
		public int ProductionRequired { get; }
		public int ProductionProgress { get; set; }
		public Dictionary<string, int> Cost { get; }
		public GameObject Product { get; }
		Commandlet Get ();
		public event Action<int, int> OnWork;
	}
}