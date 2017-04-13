using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour {
	
	[SerializeField] float time;
	[HideInInspector] public int Power;
	
	[SerializeField] Object[] DestroyOnHit;
	
	// Update is called once per frame
	void Start ()
	{
		Destroy(gameObject,time);
	}
	
	public int Hit()
	{
		foreach (var obj in DestroyOnHit)
			Destroy(obj);
		
		Destroy(gameObject, 2);
		
		return Power;
	}
}
