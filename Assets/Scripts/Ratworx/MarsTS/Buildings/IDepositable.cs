using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Buildings {

	public interface IDepositable : IUnit {
		int Deposit (string resourceKey, int depositAmount);

	}
}