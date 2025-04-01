using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Entities {

	public interface ITaggable<T> : ITaggable {
		T Get ();
	}

	public interface ITaggable {
		string Key { get; }
		Type Type { get; }
	}
}