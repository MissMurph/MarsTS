using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractEvent {
	public string Name { get; private set; }

	public AbstractEvent (string name) {
		Name = name;
	}
}