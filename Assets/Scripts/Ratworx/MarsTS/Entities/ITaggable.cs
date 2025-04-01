using System;

namespace Ratworx.MarsTS.Entities {

	public interface ITaggable<T> : ITaggable {
		T Get ();
	}

	public interface ITaggable {
		string Key { get; }
		Type Type { get; }
	}
}