#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IA))]
public class IA_Controller : Editor {
	
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		IA attachedIA =(IA)target;
		
		if(GUILayout.Button("Attack"))
			attachedIA.Attack();
		
		if(GUILayout.Button("Defend"))
			attachedIA.Defend();
	}
}
#endif