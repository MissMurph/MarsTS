using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconRenderer : MonoBehaviour {

	[SerializeField]
	private RenderDetails[] renderEntries;
}

[Serializable]
public class RenderDetails {
	public string name;
	public float viewPortSize;
}