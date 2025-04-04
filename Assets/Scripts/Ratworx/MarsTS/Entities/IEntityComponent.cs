using System;

namespace Ratworx.MarsTS.Entities {

	public interface IEntityComponent<T> : IEntityComponent {
		T Get ();
	}

	public interface IEntityComponent {
		string Key { get; }
	}
}