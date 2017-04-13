using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Algorithm_Manager : MonoBehaviour {
	
	void Start () {
		GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("Algorithm", 0) == 1;
	}
	
	
	public void SetAlternativeAlgorithm (bool INPUT)
	{
		PlayerPrefs.SetInt("Algorithm", INPUT ? 1 : 0);
	}
}
