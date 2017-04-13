using UnityEngine;
using UnityEngine.UI;

public class UpdateIP : MonoBehaviour {

	void Start () {
		GetComponent<Text>().text = "Local IP : " + Network.player.ipAddress;;
	}
}
