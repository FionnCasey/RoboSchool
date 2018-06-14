using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRotation : MonoBehaviour {

	public float sensitivity = 1f;

	private Vector2 mousePos = new Vector2();

	void Update ()
	{
		mousePos.x = Input.GetAxis("Mouse X");
		mousePos.y = Input.GetAxis("Mouse Y");

		transform.Rotate(mousePos.y, 0, mousePos.x);
	}
}
