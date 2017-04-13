using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LevelManager : MonoBehaviour
{
	[SerializeField] GameObject IA;
	[SerializeField] GameObject OfflinePlayer;
	[SerializeField] Material[] Skyboxes;
	
	Transform[] Placeholders;
	NetworkManager NM;
	int BotNum;
	GameObject own_Canvas;
	Brain[] Brains;
	Camera own_Camera;
	Skybox own_Skybox;
	int SkyboxID = 0;
	
	void Awake()
	{
		VRSettings.enabled = VRDevice.isPresent;
		InputTracking.Recenter();
	}
	
	void Update()
	{
		if(Input.GetKey(KeyCode.R))
			InputTracking.Recenter();
	}
	
	void Start()
	{
		DontDestroyOnLoad(this.gameObject);
		
		NM = GameObject.FindObjectOfType<NetworkManager>();
		own_Canvas = transform.FindChild("Menu Canvas").gameObject;
		
		Placeholders = transform.FindChild("Placeholders").GetComponentsInChildren<Transform>();
		
		own_Skybox = transform.GetComponentInChildren<Skybox>();
		own_Camera = transform.GetComponentInChildren<Camera>();
	}
	
	public void Load(string Level)
	{
		SceneManager.LoadScene(Level);
	}
	
	public Material GetSkybox()
	{
		return own_Skybox.material;
	}
	
	public void SetBotNum(int INPUT)
	{
		BotNum = INPUT;
	}
	
	public void Host()
	{
		NM.StartHost();
	}
	
	public void Join(string INPUT)
	{
		if(INPUT.Length == 0)
			NM.networkAddress = "127.0.0.1";
		else
			NM.networkAddress = INPUT;
		
		NM.StartClient();
	}
	
	public void ChangeSkybox()
	{
		int OldSkyboxID = SkyboxID;
		
		do {
			SkyboxID = Random.Range(0,Skyboxes.Length);
		} while (OldSkyboxID == SkyboxID);
		
		own_Skybox.material = Skyboxes[SkyboxID];
	}
	
	void OnLevelWasLoaded(int level)
	{
		own_Camera.enabled = (level == 1);
		own_Canvas.SetActive(level == 1);
		
		CancelInvoke("HealAll");
		
		switch (level)
		{
		case 0:
			Load("1 - Menu");
			break;
		case 2:
			Invoke("InitOffline",0);
			break;
		}
	}
	
	void InitOffline()
	{
		// Instantiate local player
		
		Instantiate(OfflinePlayer);
		
		// Setting up IAs and training heal
		
		for (int i = 1; i < BotNum +1; i++)
		{
			GameObject newIA = Instantiate(IA, Placeholders[i].position, transform.rotation) as GameObject;
			newIA.transform.LookAt(Vector3.zero);
		}
		
		Brains = FindObjectsOfType<Brain>();
		
		InvokeRepeating("HealAll",0,0.1f);
	}
	
	void HealAll()
	{
		for (int i = 0; i < BotNum + 1; i++)
			Brains[i].Heal(1);
	}
	
	public void Quit()
	{
		Application.Quit();
	}
}
