using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoguidedMissile : MonoBehaviour
{
	// follow de la cible

	[SerializeField] float range = 10f;
	[SerializeField] float force = 5f;
	[SerializeField] int angle = 90;
	[SerializeField] Vector3 offset;
	
	Rigidbody ownRb;
	Transform cible;
	float distanceCible;
	float initVelocity;
	Vector3 direction;

	void Start()
	{
		ownRb = GetComponent<Rigidbody>();
		distanceCible = range;
		initVelocity = ownRb.velocity.magnitude;
	}

	void FixedUpdate()
	{
		if (cible != null)
		{
			transform.LookAt(ownRb.velocity);

			if (Vector3.Angle(ownRb.velocity, cible.position + offset - transform.position) < angle
				&& Vector3.Distance(cible.transform.position, transform.position) < range)
			{
				direction = (cible.position + offset - transform.position).normalized * range / (cible.position + offset - transform.position).magnitude;
				ownRb.AddForce(direction * force);
				ownRb.velocity = ownRb.velocity * initVelocity / ownRb.velocity.magnitude;
			}
			else
			{
				cible = null;
				distanceCible = range;
			}
		}
		else
		{
			foreach (Collider newCible in Physics.OverlapSphere(transform.position, range))
				if ((newCible.CompareTag("Ennemy") || newCible.CompareTag("Player")) && 
					Vector3.Distance(newCible.transform.position, transform.position) < distanceCible &&
					Vector3.Angle(ownRb.velocity, newCible.transform.position - transform.position) < angle)
				{
					distanceCible = Vector3.Distance(newCible.transform.position, transform.position);
					cible = newCible.transform;
				}
		}
	}
	
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (isActiveAndEnabled)
		{
			Handles.color = new Color(255, 0, 155, 0.1f);
			Handles.DrawSolidArc(transform.position,ownRb.velocity, Quaternion.AngleAxis(-angle, Vector3.up) * ownRb.velocity, 360, range);
			Handles.DrawSolidArc(transform.position, Vector3.up, Quaternion.AngleAxis(-angle, Vector3.up) * ownRb.velocity, 2*angle, range);
			
			if (cible != null)
			{
				Handles.color = new Color(255, 0, 155, 1);
				Handles.DrawLine(cible.position + offset, transform.position);
			}
		}
	}
#endif
}