using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Indicateur : MonoBehaviour {
	
	Brain Player;
	Slider indicateur;
	
	public enum Target { HP = 0, Load = 1, Shield = 2 }
	[SerializeField] Target Selection;
	
	void Start () {
		Player = transform.parent.GetComponentInParent<Brain>();
		indicateur = GetComponent<Slider>();
	}
	
	void Update () {
		switch (Selection)
		{
		case Target.HP:
			indicateur.value = Player.GetHP();
			break;
		case Target.Load:
			indicateur.value = Player.GetShotLoad();
			break;
		case Target.Shield:
			indicateur.value = Player.GetShield();
			break;
		}
	}
}
