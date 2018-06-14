using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Brains/CartBrain")]
public class CartBrain : ScriptableObject {

	[Header("HL1")]
	[Range(-1, 1)]public float a1;
	[Range(-1, 1)]public float a2;
	[Range(-1, 1)]public float a3;
	[Range(-1, 1)]public float a4;

	[Header("HL2")]
	[Range(-1, 1)]public float b1;
	[Range(-1, 1)]public float b2;
	[Range(-1, 1)]public float b3;
	[Range(-1, 1)]public float b4;

	[Header("HL3")]
	[Range(-1, 1)]public float c1;
	[Range(-1, 1)]public float c2;
	[Range(-1, 1)]public float c3;
	[Range(-1, 1)]public float c4;

	[Header("HL4")]
	[Range(-1, 1)]public float d1;
	[Range(-1, 1)]public float d2;
	[Range(-1, 1)]public float d3;
	[Range(-1, 1)]public float d4;

	[Header("OUT")]
	[Range(-1, 1)]public float o1;
	[Range(-1, 1)]public float o2;
	[Range(-1, 1)]public float o3;
	[Range(-1, 1)]public float o4;
}
