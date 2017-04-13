using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SettingsMenu : MonoBehaviour
{
	[SerializeField] KeyCode m_enableKey = KeyCode.Escape;
	[SerializeField] GameObject[] ToDisable;
	bool m_enable;
	
	void Update () 
	{
		if(Input.GetKeyDown(m_enableKey))
		{
			m_enable = !m_enable;
			
			MonoBehaviour temp = FindObjectOfType<MouseLook>();
			
			if(temp != null)
				temp.enabled = !m_enable;
			
			temp = FindObjectOfType<PlayerController>();
			
			if(temp != null)
				temp.enabled = !m_enable;
			
			
			foreach (var obj in ToDisable)
				obj.SetActive(!m_enable);
			
			for (int i = 0; i < transform.childCount; ++i)
				transform.GetChild(i).gameObject.SetActive(m_enable);
		}
	}
	
	public void Exit()
	{
		FindObjectOfType<NetworkManager>().StopHost();
		
		FindObjectOfType<LevelManager>().Load("1 - Menu");
	}
}
