using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Buildings {

	public interface IDepositable : IUnitInterface {
		int Deposit (string resourceKey, int depositAmount);

	}
}