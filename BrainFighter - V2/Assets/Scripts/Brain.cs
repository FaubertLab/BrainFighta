using UnityEngine;
using UnityEngine.Networking;

public class Brain : NetworkBehaviour
{
	[SerializeField] bool Online;
	
	[Space,Header("Missile")]
	[SerializeField]
	GameObject Projectile;
	Transform ProjectileParent;
	float ProjectileVelocity = 500f;

	Renderer ShieldRenderer;
	
	[Space,Header("Attributs")]
	
	[SyncVar(hook = "OnChangeShield")]
	float Shield = 0;

	float ShotLoad = 0;
	[SerializeField]
	float ShotLoadTime;
	[SerializeField]
	int ShotPower = 20;
	Transform Canon;
	Rigidbody ownRgbd;
	
	[SyncVar(hook = "OnChangeHP")]
	int HP = 100;
	[SyncVar]
	float FocusValue;
	Animator HitAnim;

	Renderer SelfRenderer;
	Animator animator;
	Transform FollowCamera;

	void Start()
	{
		FollowCamera = transform.FindChild("Follow Camera Rotation");
		
		ProjectileParent = GameObject.Find("Projectiles").transform;
		
		if (isLocalPlayer || (!Online && GetComponent<PlayerController>()))
			HitAnim = FollowCamera.FindChild("UI_Hit").FindChild("Hit").GetComponent<Animator>();
		
		Canon = FollowCamera.FindChild("Canon");
		
		ShieldRenderer = FollowCamera.FindChild("Bouclier").GetComponent<Renderer>();
		SelfRenderer = FollowCamera.FindChild("BrainModel").GetComponent<Renderer>();
		animator = GetComponent<Animator>();
		ownRgbd = GetComponent<Rigidbody>();
	}

	void Update()
	{
		if (HP > 0)
		{
			UpdateShotLoad(Time.deltaTime);
		}
	}

	public void Moove(Vector3 INPUT)
	{
		if (HP > 0)
			ownRgbd.velocity = INPUT;
	}

	void OnTriggerEnter(Collider Obj)
	{
		if (Obj.gameObject.CompareTag("Missile"))
		{
			TakeDamages(Obj.transform.position, Obj.gameObject.GetComponent<Missile>().Hit());
		}
	}
	
	public void TakeDamages(Vector3 ShotPosition, int Power)
	{
		HP -= (int)(Power * (1 - Shield));
		
		if(!Online)
		{
			OnChangeHP(HP);
		}
		
		if(isLocalPlayer && HP > 0 || HitAnim != null)
		{
			Vector2 a = new Vector2(-FollowCamera.forward.x, -FollowCamera.forward.z);
			Vector2 b = new Vector2(ShotPosition.x, ShotPosition.z) - new Vector2(transform.position.x, transform.position.z);
			
			float angle = Vector3.Cross(a, b).z < 0 ? Vector3.Angle(a, b) : 360 - Vector3.Angle(a, b);
			
			if(angle > 45 && angle < 135)
				HitAnim.SetTrigger("Left");
			else if(angle > 135 && angle < 225)
				HitAnim.SetTrigger("Front");
			else if(angle > 225 && angle < 315)
				HitAnim.SetTrigger("Right");
			else if(angle > 315 || angle < 45)
				HitAnim.SetTrigger("Back");
		}
	}
	
	public void SetShield(float INPUT)
	{
		Shield = INPUT;
		
		if(!Online)
			OnChangeShield(INPUT);
		else if (isClient)
			CmdOnChangeShield(INPUT);
	}
	
	[Command]
	void CmdOnChangeShield(float INPUT_Shield)
	{
		Shield = INPUT_Shield;
		
		if (Shield > 1)
			Shield = 1;
		else if (Shield < 0)
			Shield = 0;
		
		Color newColor = ShieldRenderer.material.color;
		newColor.a = Shield / 1f;
		ShieldRenderer.material.color = newColor;
	}
	
	void OnChangeShield(float INPUT_Shield)
	{
		Shield = INPUT_Shield;
		
		if (Shield > 1)
			Shield = 1;
		else if (Shield < 0)
			Shield = 0;
		
		Color newColor = ShieldRenderer.material.color;
		newColor.a = Shield / 1f;
		ShieldRenderer.material.color = newColor;
	}
	
	void OnChangeHP (int INPUT_HP)
	{
		SelfRenderer.material.color = new Color(1, INPUT_HP / 100f, INPUT_HP / 100f, 1);
		
		HP = INPUT_HP;
		
		if (HP <= 0)
		{
			animator.Play("Explosion");
			
			FollowCamera.gameObject.SetActive(false);
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Collider>().enabled = false;
			
			if(GetComponent<IA>())
				GetComponent<IA>().enabled = false;
			
			if(hasAuthority)
			{
				GameObject.Find("UI_Indicateurs").SetActive(false);
				GameObject.Find("Gameover").SetActive(true);
			}
		}
		else
		{
			FollowCamera.gameObject.SetActive(true);
			GetComponent<Rigidbody>().isKinematic = false;
			GetComponent<Collider>().enabled = true;
			
			if(hasAuthority)
			{
				GameObject.Find("UI_Indicateurs").SetActive(true);
				GameObject.Find("Gameover").SetActive(false);
			}
		}
	}

	public void Heal(int INPUT)
	{
		HP += INPUT;
		
		if(HP > 100)
			HP = 100;
		
		SelfRenderer.material.color = new Color(1, HP / 100f, HP / 100f, 1);
	}

	[Command]
	void CmdSetFocusValue(float INPUT)
	{
		FocusValue = INPUT;
	}
	
	public void SetFocusValue(float INPUT)
	{
		if(Online && isClient)
			CmdSetFocusValue(INPUT);
		
		FocusValue = INPUT;
	}
	
	[Command]
	void CmdSetHP(int INPUT)
	{
		HP = INPUT;
	}

	void UpdateShotLoad(float ElapsedTime)
	{
		if (FocusValue > 0)
		{
			if (ShotLoad < 1)
				ShotLoad += ElapsedTime / ShotLoadTime;
			else
			{
				if(isLocalPlayer)
					CmdShoot();
				else if(!Online)
					Shoot();
				
				ShotLoad = 0;
			}
		}
		else
		{
			// Dans ce cas FocusValue<0
			
			if (ShotLoad > 0)
				ShotLoad -= ElapsedTime / ShotLoadTime;
		}

		animator.Play("Attack", 0, ShotLoad);
	}
	
	[Command]
	void CmdShoot()
	{
		NetworkServer.Spawn(Shoot());
	}
	
	GameObject Shoot()
	{
		GameObject shot = Instantiate(Projectile, Canon.position, Canon.rotation) as GameObject;
		shot.GetComponent<Rigidbody>().velocity = FollowCamera.forward * ProjectileVelocity + ownRgbd.velocity / 3;
		shot.GetComponent<Missile>().Power = ShotPower;
		shot.transform.SetParent(ProjectileParent);
		
		FocusValue = 0;
		
		return shot;
	}
	
	public void Respawn()
	{
		if(HP <= 0)
		{
			NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition>();
			
			if (isLocalPlayer)
			{
            // Set the spawn point to origin as a default value
				Vector3 spawnPoint = Vector3.zero;
				
            // If there is a spawn point array and the array is not empty, pick one at random
				if (spawnPoints != null && spawnPoints.Length > 0)
					spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
				
            // Set the player’s position to the chosen spawn point
				transform.position = spawnPoint;
			}
			
			if(isClient)
				CmdSetHP(100);
			else
				HP = 100;
		}
	}
	
	public float GetShield() {return Shield;}
	public float GetShotLoad() {return ShotLoad;}
	public int GetHP() {return HP;}
}
