using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SetUserName : MonoBehaviour {
	
	[SerializeField] Text NamePreview;
	[SerializeField] RawImage ColorPreview;
	[SerializeField] Slider Slider;
	
	void Start()
	{
		string PlayerName = PlayerPrefs.GetString("PlayerName", "Anonymous");
		float PlayerNameColor = PlayerPrefs.GetFloat("PlayerNameColor", Random.value);
		
		PlayerPrefs.SetString("PlayerName", PlayerName);
		PlayerPrefs.SetFloat("PlayerNameColor", PlayerNameColor);
		
		NamePreview.text = PlayerName;
		ColorPreview.color = Color.HSVToRGB(PlayerNameColor,1,1);
		Slider.value = PlayerNameColor;
	}
	
	public void SetName (string INPUT)
	{
		PlayerPrefs.SetString("PlayerName", INPUT);
	}
	
	public void SetColor (float INPUT)
	{
		Color newColor = Color.HSVToRGB(INPUT,1,1);
		
		ColorPreview.color = newColor;
		PlayerPrefs.SetFloat("PlayerNameColor", INPUT);
	}
}
