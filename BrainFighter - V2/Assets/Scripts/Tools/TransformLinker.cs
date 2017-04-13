using UnityEngine;
using System.Collections;

public class TransformLinker : MonoBehaviour {
	
	[SerializeField] GameObject Reference;
	[SerializeField] Vector3 PositionFactor;
	[SerializeField] Vector3 RotationFactor;
	
	// Update is called once per frame
	void Update () {
		
		if(Reference)
		{
			transform.localPosition = Vector3.Scale(Reference.transform.localPosition, PositionFactor);
			transform.localEulerAngles = Vector3.Scale(Reference.transform.localEulerAngles, RotationFactor);
		}
	}
}
