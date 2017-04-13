using UnityEngine;
using System.Collections;

public class FacingCamera : MonoBehaviour {

	void Update () {
		transform.LookAt(Camera.main.transform);
	}
}
