namespace Ratworx.MarsTS.Buildings {

	public class Scrapyard : Factory, IDepositable {
		
		public int Deposit (string resourceKey, int depositAmount) {
			if (Owner.GetResource(resourceKey).Deposit(depositAmount)) return depositAmount;
			else return 0;
		}
	}
}