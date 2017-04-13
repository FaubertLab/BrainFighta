using UnityEngine;
using System.Collections;

public class Follow_Y_Rotation : MonoBehaviour {
	
	[SerializeField] Transform Target;
	
	void Start()
	{
		if(Target == null)
		{
			Destroy(this);
		}
			
	}
	
	void Update () {
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,
												Target.transform.localEulerAngles.y,
												transform.localEulerAngles.z);
	}
}
