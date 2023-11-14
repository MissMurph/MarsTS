using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

	public interface IRegistryObject<T> : IRegistryObject {
		T Get ();
		IRegistryObject<T> GetRegistryEntry ();
	}

	public interface IRegistryObject {
		string RegistryType { get; }
		string RegistryKey { get; }
    }
}