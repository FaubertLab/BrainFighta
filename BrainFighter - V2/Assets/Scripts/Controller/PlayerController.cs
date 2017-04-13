using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.VR;

public class PlayerController : NetworkBehaviour {
	
	[SerializeField] bool Online;
	[SerializeField] float FocusRate;
	[SerializeField] float Vitesse;
	
	[SerializeField] GameObject IA;
	
	[SerializeField] Object[] DestroyOnServer;
	
	Brain playerBrain;
	
	Transform ModelTransform;
	
	void Start ()
	{
		if (!isLocalPlayer && Online)
		{
			foreach(Object i in DestroyOnServer)
				Destroy(i);
			
			transform.FindChild("Follow Camera Rotation").FindChild("BrainModel").gameObject.layer = LayerMask.GetMask("Default");
			gameObject.tag = "Ennemy";
			
			Destroy(GetComponent<PlayerController>());
			Destroy(GetComponent<MouseLook>());
		}
		// Mode souris
		else if(VRSettings.enabled)
		{
			transform.FindChild("Follow Camera Rotation").FindChild("UI_Hit").FindChild("Hit").GetComponent<RectTransform>().sizeDelta = new Vector2(-500,-100);
			
			Destroy(GetComponent<MouseLook>());
		}
		
		if(FindObjectOfType<MUSE_IO_READER>().IsRunning)
			Destroy(this);
		
		playerBrain = GetComponent<Brain>();
		ModelTransform = transform.FindChild("Follow Camera Rotation").GetComponent<Transform>();
		
		if(isLocalPlayer || !Online)
		{
			if(PlayerPrefs.GetInt("Algorithm", 0) == 1)
				FindObjectOfType<Algorithms>().AddCallBacks(playerBrain.SetFocusValue, playerBrain.SetShield);
			else
				MUSE_DATAS.Instance.AddCallBacks(playerBrain.SetFocusValue, playerBrain.SetShield);
			
			GameObject.Find("CenterEyeAnchor").GetComponent<Skybox>().material = GameObject.Find("Settings").GetComponent<LevelManager>().GetSkybox();
		}
	}
	
	void Update ()
	{
		if (isLocalPlayer || !Online)
		{
			// Focus and relax
			if (Input.GetAxis("Fire1") > 0)
				playerBrain.SetFocusValue(Input.GetAxis("Fire1") * FocusRate);
			else
				playerBrain.SetFocusValue(-FocusRate);
			
			playerBrain.SetShield(Input.GetAxis("Fire2"));
			
			
			if(Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return))
				playerBrain.Respawn();
			
			// Movement controls
			Vector3 rotatedVector = Quaternion.AngleAxis(ModelTransform.eulerAngles.y, Vector3.up) * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			
			playerBrain.Moove(rotatedVector * Vitesse);
		}
	}
}
