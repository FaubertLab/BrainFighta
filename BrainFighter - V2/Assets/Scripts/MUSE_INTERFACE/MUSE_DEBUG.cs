using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Linq;

public class MUSE_DEBUG : MonoBehaviour {
	
	[SerializeField] KeyCode m_enableKey = KeyCode.Tab;
	[SerializeField] Color BackgroundColor = Color.grey;
	bool m_enable;
	
	const int m_margin = 20;
	const int m_slidersWidth = 20;
	const int m_toolbarHeight = 30;
	const int m_contentMargin = 10;
	
	const int m_infos_upperPanelMaxHeight = 140;
	const int m_infos_slidersPanelMaxHeight = 60;
	
	const int m_minZoomValue = 100;
	const int m_maxZoomValue = 1000;
	
	Texture2D BackgroundTexture;
	Rect m_bound_Background;
	Rect m_bound_Toolbar;
	Rect m_bound_Panel;
	Rect m_bound_Panel_DivisionTMP;
	Rect m_bound_Panel_DivisionTMPBis;
	GUIStyle Txt_Style;
	GUIStyle Txt_Warning_Style;
	string OutputTxt;
	string ConsoleTxt;
	string tmp_Text = "";
	
	int m_section;
	int m_FFT_Selection;
	float m_zoomSliderValue = m_minZoomValue;
	string[] m_toolbarChoicesList = new string[] {"Infos", "Console", "EEG Plot", "FFT Histogram"};
	bool m_toggleLogging = false;
	
	EEGVisualizer[] m_visualizers_EEG;
	FFTVisualizer m_visualizer_FFT;
	MUSE_IO_READER m_museIOReader;
	Algorithms m_Algorithm;
	
	void Start () {
		
		// Getting Muse_IO_READER and Algorithms
		
		m_museIOReader = GetComponent<MUSE_IO_READER>();
		m_Algorithm = GetComponent<Algorithms>();
		
		// Setting debug windows properties
		
		BackgroundTexture = new Texture2D(1, 1);
		BackgroundTexture.SetPixel(0, 0, BackgroundColor);
		BackgroundTexture.Apply();
		
		Txt_Style =  new GUIStyle();
		Txt_Style.wordWrap = true;
		Txt_Style.richText = false;
		Txt_Style.normal.textColor = Color.white;
		Txt_Style.clipping = TextClipping.Clip;
		
		Txt_Warning_Style =  new GUIStyle();
		Txt_Warning_Style.wordWrap = false;
		Txt_Warning_Style.richText = false;
		Txt_Warning_Style.normal.textColor = Color.yellow;
		Txt_Warning_Style.clipping = TextClipping.Clip;
		Txt_Warning_Style.alignment = TextAnchor.UpperCenter;
		
		// Creating visualizers
		
		m_visualizers_EEG = new EEGVisualizer[4];
		
		for (int i = 0; i < 4; i++) {
			m_visualizers_EEG[i] = gameObject.AddComponent<EEGVisualizer>();
			m_visualizers_EEG[i].Initialize(2000, i);
		}
		
		m_visualizer_FFT = gameObject.AddComponent<FFTVisualizer>();
		m_visualizer_FFT.Initialize();
		
		// Writting in the console
		
		WriteLine("Hello, muse-io logs will be printed here : \n");
	}
	
	void Update()
	{
		if(Input.GetKeyDown(m_enableKey))
		{
			m_enable = !m_enable;
			
			if(FindObjectOfType<MouseLook>() != null)
				FindObjectOfType<MouseLook>().enabled = !m_enable;
			if(FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
				FindObjectOfType<UnityEngine.EventSystems.EventSystem>().enabled = !m_enable;
			
			if(!m_museIOReader.IsRunning && FindObjectOfType<PlayerController>() != null)
				FindObjectOfType<PlayerController>().enabled = !m_enable;
				
		}
	}
	
	public void WriteLine(string NewLine)
	{
		ConsoleTxt += NewLine +"\n> ";
	}
	
	void OnGUI()
	{
		if(MUSE_DATAS.Instance.Headband.Sum() > 4)
			GUI.Box(new Rect(0,0,Screen.width, Screen.height), "/!\\ Bad connection", Txt_Warning_Style);
		
		if(m_enable)
		{
			GUI.skin.box.normal.background = BackgroundTexture;
			
			m_bound_Background = Rect.MinMaxRect(m_margin, m_margin, Screen.width - m_margin, Screen.height - m_margin);
			GUI.Box(m_bound_Background, GUIContent.none);
			
			m_bound_Toolbar = Rect.MinMaxRect(m_bound_Background.xMin,
												m_bound_Background.yMin,
												m_bound_Background.xMax,
												m_bound_Background.yMin + m_toolbarHeight);
			
			m_bound_Panel = Rect.MinMaxRect(m_bound_Toolbar.xMin + m_contentMargin,
											m_bound_Toolbar.yMax + m_contentMargin,
											m_bound_Toolbar.xMax - m_contentMargin,
											m_bound_Background.yMax - m_contentMargin);
			
			switch (m_section = GUI.Toolbar(m_bound_Toolbar, m_section, m_toolbarChoicesList))
			{
			case 0:
				// Infos
				Section_Infos();
				break;
			case 1:
				// Console
				Section_Console();
				break;
			case 2:
				// EEG Plot
				Section_EEG();
				break;
			case 3:
				// FFT Histogram
				Section_FFT();
				break;
			default:
				break;
			}
		}
	}
	
	void Section_Infos()
	{
		Txt_Style.alignment = TextAnchor.MiddleCenter;
		
		OutputTxt = "Connected to : " + MUSE_DATAS.Instance.Config.serial_number  + "\n\n"
					+ "Charge : " + MUSE_DATAS.Instance.Batt[0] / 100f + " %\n"
					+ "Voltage (Fuel Gauge) : " + MUSE_DATAS.Instance.Batt[1] + " mV\n"
					+ "Voltage (ADC) : " + MUSE_DATAS.Instance.Batt[2] + " mV\n"
					+ "Temperature : " + MUSE_DATAS.Instance.Batt[3] + " C";
		
		m_bound_Panel_DivisionTMP = new Rect(m_bound_Panel.xMin,
											m_bound_Panel.yMin,
											m_bound_Panel.width / 2,
											Mathf.Min(m_bound_Panel.height, m_infos_upperPanelMaxHeight));
		
		GUI.Box(m_bound_Panel_DivisionTMP, OutputTxt, Txt_Style);
		
		for (int i = 0; i < 4; i++)
		{
			m_bound_Panel_DivisionTMPBis = new Rect(
				m_bound_Panel_DivisionTMP.xMax,
				m_bound_Panel_DivisionTMP.yMin,
				m_bound_Panel_DivisionTMP.width/2,
				m_bound_Panel_DivisionTMP.height/2);
			
			switch (i)
			{
			case 0:
				tmp_Text = "[0] Left Ear";
				m_bound_Panel_DivisionTMPBis.y += m_bound_Panel_DivisionTMP.height/2;
				break;
			case 1:
				tmp_Text = "[1] Left Forehead";
				break;
			case 2:
				tmp_Text = "[2] Right Forehead";
				m_bound_Panel_DivisionTMPBis.x += m_bound_Panel_DivisionTMP.width/2;
				break;
			case 3:
				tmp_Text = "[3] Right Ear";
				m_bound_Panel_DivisionTMPBis.y += m_bound_Panel_DivisionTMP.height/2;
				m_bound_Panel_DivisionTMPBis.x += m_bound_Panel_DivisionTMP.width/2;
				break;
			default:
				break;
			}
			
			switch ((int)MUSE_DATAS.Instance.Headband[i])
			{
			case 1:
				Txt_Style.normal.textColor = Color.green;
				break;
			case 2:
				Txt_Style.normal.textColor = Color.yellow;
				break;
			case 3:
			case 4:
				Txt_Style.normal.textColor = Color.red;
				break;
			}
			
			GUI.Box(m_bound_Panel_DivisionTMPBis,tmp_Text, Txt_Style);
			
			Txt_Style.normal.textColor = Color.white;
		}
		
		Txt_Style.alignment = TextAnchor.MiddleCenter;
		
		m_bound_Panel_DivisionTMP = new Rect(m_bound_Panel.xMin,
								m_bound_Panel_DivisionTMP.yMax,
								m_bound_Panel.width,
			Mathf.Min(m_infos_slidersPanelMaxHeight,Mathf.Max(m_bound_Panel.height - m_bound_Panel_DivisionTMP.height, 0)));
		
		for (int ligne = 0; ligne < 3; ligne++)
			for (int colonne = 0; colonne < 3; colonne++)
			{
				m_bound_Panel_DivisionTMPBis = new Rect(
					m_bound_Panel_DivisionTMP.xMin + (2 * colonne - 1) * (m_bound_Panel_DivisionTMP.width / 5),
					m_bound_Panel_DivisionTMP.yMin + ligne * (m_bound_Panel_DivisionTMP.height / 3),
					m_bound_Panel_DivisionTMP.width / 5 - 20,
					m_bound_Panel_DivisionTMP.height / 3);
				
				switch (colonne)
				{
				case 0:
					m_bound_Panel_DivisionTMPBis.x = m_bound_Panel_DivisionTMP.xMin;
					
					switch (ligne)
					{
					case 1:
						GUI.Box(m_bound_Panel_DivisionTMPBis, "Muse Algorithm", Txt_Style);
						break;
					case 2:
						GUI.Box(m_bound_Panel_DivisionTMPBis, "Custom Algorithm", Txt_Style);
						break;
					}
					break;
				case 1:
					m_bound_Panel_DivisionTMPBis.width *= 2;
					switch (ligne)
					{
					case 0:
						GUI.Box(m_bound_Panel_DivisionTMPBis, "Mellow", Txt_Style);
						break;
					case 1:
						GUI.HorizontalSlider (m_bound_Panel_DivisionTMPBis, MUSE_DATAS.Instance.Mellow, 0f, 1f);
						break;
					case 2:
						GUI.HorizontalSlider (m_bound_Panel_DivisionTMPBis, m_Algorithm.RelaxedValue, 0f, 1f);
						break;
					}
					break;
				case 2:
					m_bound_Panel_DivisionTMPBis.width *= 2;
					switch (ligne)
					{
					case 0:
						GUI.Box(m_bound_Panel_DivisionTMPBis, "Concentration", Txt_Style);
						break;
					case 1:
						GUI.HorizontalSlider (m_bound_Panel_DivisionTMPBis, MUSE_DATAS.Instance.Concentration, 0f, 1f);
						break;
					case 2:
						GUI.HorizontalSlider (m_bound_Panel_DivisionTMPBis, m_Algorithm.FocusedValue, 0f, 1f);
						break;
					}
					break;
				}
			}
	}
	
	void Section_Console()
	{
		Txt_Style.alignment = TextAnchor.LowerLeft;
		OutputTxt = ConsoleTxt;
		
		GUI.Box(m_bound_Panel, OutputTxt, Txt_Style);
		
		// Launch MUSE button
		
		if(!m_museIOReader.IsRunning)
		{
			
			
			m_bound_Panel_DivisionTMP = Rect.MinMaxRect(m_bound_Panel.center.x - 100,
														m_bound_Panel.center.y - 50,
														m_bound_Panel.center.x + 100,
														m_bound_Panel.center.y + 50);
			
			m_bound_Panel_DivisionTMPBis = Rect.MinMaxRect(m_bound_Panel_DivisionTMP.xMin,
															m_bound_Panel_DivisionTMP.yMin + 100,
															m_bound_Panel_DivisionTMP.xMax,
															m_bound_Panel_DivisionTMP.yMax + 100);
			
			if (GUI.Button(m_bound_Panel_DivisionTMP, "Launch muse-io"))
			{
				GetComponent<MUSE_IO_READER>().Launch();
				if(m_toggleLogging)
					GetComponent<MUSE_DATAS>().StartLogging();
			}
			else
				m_toggleLogging = GUI.Toggle(m_bound_Panel_DivisionTMPBis, m_toggleLogging, " Logging EEG datas");
			
			if(FindObjectOfType<PlayerController>() != null)
				FindObjectOfType<PlayerController>().enabled = !m_enable;
		}
	}
	
	void Section_EEG()
	{
		m_bound_Panel_DivisionTMP = Rect.MinMaxRect(m_bound_Panel.xMin,
													m_bound_Panel.yMin,
													m_bound_Panel.xMin + m_slidersWidth,
													m_bound_Panel.yMax);
		
		m_zoomSliderValue = GUI.VerticalSlider (m_bound_Panel_DivisionTMP,
												m_zoomSliderValue,
												m_maxZoomValue,
												m_minZoomValue);
		
		for (int i = 0; i < m_visualizers_EEG.Length; i++)
		{
			m_visualizers_EEG[i].Resize(new Vector2(
										(Screen.width + m_slidersWidth) / 2,
										(i + 0.5f) * (m_bound_Panel.height / 4) + m_margin + m_contentMargin),
										m_bound_Panel.width + m_contentMargin - m_slidersWidth,
										m_zoomSliderValue);
			
			m_visualizers_EEG[i].Draw();
		}
	}
	
	void Section_FFT()
	{
		m_bound_Panel_DivisionTMP = Rect.MinMaxRect(m_bound_Panel.xMin,
													m_bound_Panel.yMax - m_toolbarHeight,
													m_bound_Panel.xMax,
													m_bound_Panel.yMax);
		
		switch (m_visualizer_FFT.GetChannel())
		{
		case 0:
			tmp_Text = "[0] Left Ear";
			break;
		case 1:
			tmp_Text = "[1] Left Forehead";
			break;
		case 2:
			tmp_Text = "[2] Right Forehead";
			break;
		case 3:
			tmp_Text = "[3] Right Ear";
			break;
		}
		
		if (GUI.Button(m_bound_Panel_DivisionTMP, tmp_Text))
			m_visualizer_FFT.SetChannel((m_visualizer_FFT.GetChannel() + 1) % 4);
		
		m_visualizer_FFT.Resize(new Vector2(
								(Screen.width) / 2,
								(0.5f) * m_bound_Panel.height + m_margin + m_contentMargin + m_toolbarHeight/4),
								m_bound_Panel.width + m_contentMargin,
								m_bound_Panel.height - m_toolbarHeight/2);
		
		m_visualizer_FFT.Draw();
	}
}
