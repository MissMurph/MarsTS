using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

	public interface IDepositable {
		GameObject GameObject { get; }
		int Deposit (string resourceKey, int depositAmount);

	}
}