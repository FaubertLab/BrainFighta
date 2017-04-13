using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

enum Moove {IDLE, Attack, Defend};

[RequireComponent (typeof (Brain))]
public class IA : MonoBehaviour {
	
	Brain brain;
	
	[Space,Header("Prise de decision")]
	[SerializeField] bool Online;
	[SerializeField] bool NewTargetOnFire;
	[SerializeField] float timeMin;
	[SerializeField] float timeMax;
	Moove nextMoove;
	
	[Space,Header("Caracteristiques")]
	[SerializeField] float shieldTime;
	[SerializeField] float shieldPower;
	[SerializeField] float shotPower;
	float shieldValue = 0;
	bool activateShield;
	
	[Space,Header("Detection")]
	[SerializeField] float detectionRange;
	[SerializeField] float detectionAngle;
	[SerializeField] float acceleration;
	[SerializeField] float lookAtSpeed;
	Quaternion rotation;
	Rigidbody ownRb;
	Vector3 objectif;
	Vector3 direction;
	Transform oldCible;
	Transform cible;
	
	void Start ()
	{
		nextMoove = GetRandomMoove();
		brain = GetComponent<Brain>();
		ownRb = GetComponent<Rigidbody>();
		
		SetNewDirection();
	}
	
	void Update ()
	{
		if(nextMoove == Moove.Defend)
		{
			Invoke("Defend", Random.Range(timeMin, timeMax));
			nextMoove = Moove.IDLE;
		}
		else if(nextMoove == Moove.Attack)
		{
			Invoke("Attack", Random.Range(timeMin, timeMax));
			nextMoove = Moove.IDLE;
		}
		
		if(activateShield && shieldValue < 1)
			shieldValue += Time.deltaTime / shieldTime;
		else if(shieldValue > 0)
			shieldValue -= Time.deltaTime / shieldTime;
		
		brain.SetShield(shieldValue);
		
		FollowTarget();
	}
	
	void FollowTarget()
	{
		if (cible != null)
		{
			if (Vector3.Angle(transform.forward, cible.position - transform.position) < detectionAngle
				&& Vector3.Distance(cible.transform.position, transform.position) < detectionRange)
			{
				rotation = Quaternion.LookRotation(cible.position - transform.position);
			}
			else
			{
				cible = null;
			}
		}
		else
		{
			rotation = Quaternion.LookRotation(direction - transform.position);
			
			foreach (Collider newCible in Physics.OverlapSphere(transform.position, detectionRange))
				if (newCible.gameObject != gameObject
					&& newCible.transform != oldCible
					&& (newCible.CompareTag("Ennemy") || newCible.CompareTag("Player"))
					&& Vector3.Distance(newCible.transform.position, transform.position) < detectionRange
					&& Vector3.Angle(transform.forward, newCible.transform.position - transform.position) < detectionAngle)
				{
					cible = newCible.transform;
					SetNewDirection();
				}
			
			oldCible = null;
		}
		
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * lookAtSpeed);
		
		if(Vector3.Distance(transform.position,objectif) > 100)
		{
			direction = objectif - transform.position;
			direction.Normalize();
			ownRb.AddForce(direction * acceleration);
		}
		else
			SetNewDirection();
	}
	
	void SetNewDirection()
	{
		if(cible != null)
			objectif = cible.position + new Vector3(Random.Range(-detectionRange / 2,detectionRange / 2),0, Random.Range(-detectionRange / 2,detectionRange / 2));
		else
			objectif = new Vector3(Random.Range(-2000,2000),0, Random.Range(-2000,2000));
	}
	
	public void Attack()
	{
		brain.SetFocusValue(shotPower);
		
		if(NewTargetOnFire)
		{
			oldCible = cible;
			cible = null;
		}
		
		nextMoove = GetRandomMoove();
	}
	
	public void Defend()
	{
		activateShield = true;
		Invoke("CancelDefend", shieldTime);
	}
	
	void CancelDefend()
	{
		activateShield = false;
		nextMoove = GetRandomMoove();
	}
	
	static Moove GetRandomMoove()
	{
		System.Array A = System.Enum.GetValues(typeof(Moove));
		Moove V = (Moove)A.GetValue(UnityEngine.Random.Range(1,A.Length));
		return V;
	}
	
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (isActiveAndEnabled)
		{
			Handles.color = new Color(1,0,0,0.1f);
			Handles.DrawSolidArc(transform.position, transform.up, Quaternion.AngleAxis(-detectionAngle, transform.up) * transform.forward, detectionAngle * 2, detectionRange);
			
			if (cible != null)
			{
				Handles.color = new Color(1,0,0,1);
				Handles.DrawLine(cible.position, transform.position);
			}
		}
	}
#endif
}
