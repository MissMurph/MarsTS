using System.Collections;
using System.Collections.Generic;
using MarsTS.Units;
using UnityEngine;

namespace MarsTS.Buildings {

	public interface IDepositable : IUnit {
		int Deposit (string resourceKey, int depositAmount);

	}
}