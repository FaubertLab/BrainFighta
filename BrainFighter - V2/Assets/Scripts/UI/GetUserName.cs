using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class GetUserName : NetworkBehaviour {
	
	[SerializeField] TextMesh UserNameMesh;
	
	void Start()
	{
		SetAndSyncNameAndColor();
	}
	
	public void SetAndSyncNameAndColor()
	{
		if(hasAuthority)
		{
			UserNameMesh.color = Color.HSVToRGB(PlayerPrefs.GetFloat("PlayerNameColor"),1,1);
			UserNameMesh.text = PlayerPrefs.GetString("PlayerName");
			
			if (isClient && !isServer)
			{
				CmdSetName(UserNameMesh.text);
				CmdSetColor(UserNameMesh.color);
			}
		}
	}
	
	[ClientRpc]
	void RpcSetName(string INPUT)
	{
		UserNameMesh.text = INPUT;
	}
	
	[ClientRpc]
	void RpcSetColor(Color INPUT)
	{
		UserNameMesh.color = INPUT;
	}
	
	[Command]
	void CmdSetName(string INPUT)
	{
		UserNameMesh.text = INPUT;
		
		if(isServer)
			foreach (var BrainName in FindObjectsOfType<GetUserName>())
				if(BrainName != this)
					BrainName.RpcSetName(BrainName.GetComponentInChildren<TextMesh>().text);
	}
	
	[Command]
	void CmdSetColor(Color INPUT)
	{
		UserNameMesh.color  = INPUT;
		
		if(isServer)
			foreach (var BrainName in FindObjectsOfType<GetUserName>())
				if(BrainName != this)
					BrainName.RpcSetColor(BrainName.GetComponentInChildren<TextMesh>().color);
	}
}
