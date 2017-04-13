using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]	class FloatCallback : UnityEvent<float> {}
[Serializable]	class FloatArrayCallback : UnityEvent<float[]> {}
[Serializable]	class Float2DArrayCallback : UnityEvent<float[][]> {}

public struct MuseConfig {
	public string mac_addr;
	public string serial_number;
	public string preset;
};

/*
	Description :
	http://developer.choosemuse.com/research-tools/available-data
	https://sites.google.com/a/interaxon.ca/muse-developer-site/museio/osc-paths
*/

public class MUSE_DATAS : Singleton<MUSE_DATAS>
{
	protected MUSE_DATAS () {}
	
	bool EnableCallBacks;
	
	[SerializeField] MUSE_LOGGING EEG_Logger;
	bool isLogging = false;
	
	[Header("Callbacks"),Space]
	[SerializeField] Float2DArrayCallback Raw_fft_Callback;
	[SerializeField] FloatCallback	ConcentrationCallback;
	[SerializeField] FloatCallback	MellowCallback;
	
	List<EEGVisualizer>  EEG_Visualizers;
	
	[NonSerialized] public MuseConfig Config;
	
	[NonSerialized] public float[]	EEG;
	[NonSerialized] public int[]	EEG_Quantization;
	[NonSerialized] public int[]	EEG_Timestamp;
	[NonSerialized] public int[]	EEG_Dropped_samples;
	
	[NonSerialized]	public float[]	Accelerometer; // order x,y,z
	[NonSerialized] public int[]	Accelerometer_Dropped_samples;
	
	[NonSerialized] float[]	Raw_fft0;
	[NonSerialized] float[]	Raw_fft1;
	[NonSerialized] float[]	Raw_fft2;
	[NonSerialized] float[]	Raw_fft3;
	[NonSerialized] public float[][]	Raw_fft;
	
	[NonSerialized] public float[]	Absolute_low_freqs;
	[NonSerialized] public float[]	Absolute_delta;
	[NonSerialized] public float[]	Absolute_theta;
	[NonSerialized] public float[]	Absolute_alpha;
	[NonSerialized] public float[]	Absolute_beta;
	[NonSerialized] public float[]	Absolute_gamma;
	
	[NonSerialized] public float[]	Relative_delta;
	[NonSerialized] public float[]	Relative_theta;
	[NonSerialized] public float[]	Relative_alpha;
	[NonSerialized] public float[]	Relative_beta;
	[NonSerialized] public float[]	Relative_gamma;
	
	[NonSerialized] public float[]	Score_delta;
	[NonSerialized] public float[]	Score_theta;
	[NonSerialized] public float[]	Score_alpha;
	[NonSerialized] public float[]	Score_beta;
	[NonSerialized] public float[]	Score_gamma;
	
	[NonSerialized] public int		Headband_Forehead;
	[NonSerialized] public float[]	Headband;
	[NonSerialized] public int[]	Headband_Strict;
	
	[NonSerialized] public int		Blink;
	[NonSerialized] public int		JawClench;
	
	[NonSerialized] public float	Concentration;
	[NonSerialized] public float	Mellow;
	
	[NonSerialized] public int[]	Batt;
	
	[NonSerialized] public float[]	DRLRef;

	void Awake ()
	{
		EEG = new float[4];
		EEG_Quantization = new int[4];
		EEG_Timestamp = new int[2];
		EEG_Dropped_samples = new int[4];
		
		EEG_Visualizers = new List<EEGVisualizer>();
		
		Accelerometer = new float[3];
		Accelerometer_Dropped_samples = new int[3];
		
		Raw_fft0 = new float[129];
		Raw_fft1 = new float[129];
		Raw_fft2 = new float[129];
		Raw_fft3 = new float[129];
		
		Raw_fft = new float[][] {Raw_fft0,Raw_fft1,Raw_fft2,Raw_fft3};
		
		Absolute_low_freqs = new float[4];
		Absolute_delta = new float[4];
		Absolute_theta = new float[4];
		Absolute_alpha = new float[4];
		Absolute_beta = new float[4];
		Absolute_gamma = new float[4];
		
		Relative_delta = new float[4];
		Relative_theta = new float[4];
		Relative_alpha = new float[4];
		Relative_beta = new float[4];
		Relative_gamma = new float[4];
		
		Score_delta = new float[4];
		Score_theta = new float[4];
		Score_alpha = new float[4];
		Score_beta = new float[4];
		Score_gamma = new float[4];
		
		Headband = new float[4];
		Headband_Strict = new int[4];
		
		Batt = new int[4];
		
		DRLRef = new float[2];
	}
	
	public bool StartLogging()
	{
		if(EEG_Logger != null)
		{
			EEG_Logger.CreateNewLog();
			EEG_Logger.Log("TimeStamp;Channel 0;Channel 1;Channel 2;Channel 3;");
			isLogging = true;
		}
		
		return isLogging;
	}
	
	public void AddCallBacks(UnityAction<float> Concentration, UnityAction<float> Mellow)
	{
		ConcentrationCallback.AddListener(Concentration);
		MellowCallback.AddListener(Mellow);
	}
	
	void Update()
	{
		if(EnableCallBacks)
		{
			ConcentrationCallback.Invoke(Concentration);
			MellowCallback.Invoke(Mellow);
			Raw_fft_Callback.Invoke(Raw_fft);
		}
	}
	
	public void SubscribeVisualizers(EEGVisualizer INPUT)
	{
		EEG_Visualizers.Add(INPUT);
	}
	
	public void DataAttribution(string ADRESS, object[] ARGUMENTS)
	{
		EnableCallBacks = true;
		
		switch (ADRESS)
		{
		// EEG
		case "/muse/eeg":
			for (int i = 0; i < 4; i++)
				EEG[i] = (float)ARGUMENTS.GetValue(i);
			
			EEG_Timestamp[0] = (int)ARGUMENTS.GetValue(4);
			EEG_Timestamp[1] = (int)ARGUMENTS.GetValue(5);
			
			if(isLogging)
				EEG_Logger.Log(EEG_Timestamp[0] + "." + EEG_Timestamp[1] + ";"
				+ EEG[0] + ";"
				+ EEG[1] + ";"
				+ EEG[2] + ";"
				+ EEG[3] + ";");
			
			foreach (var visualizer in EEG_Visualizers)
				visualizer.UpdateHistory(EEG);
			
			break;
		case "/muse/eeg/quantization":
			for (int i = 0; i < 4; i++)
				EEG_Quantization[i] = (int)ARGUMENTS.GetValue(i);
			break;
		case "/muse/eeg/dropped_samples":
			for (int i = 0; i < 4; i++)
				EEG_Dropped_samples[i] = (int)ARGUMENTS.GetValue(i);
			break;
			
		// ACC
		case "/muse/acc":
			for (int i = 0; i < 3; i++)
				Accelerometer[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/acc/dropped_samples":
			for (int i = 0; i < 3; i++)
				Accelerometer_Dropped_samples[i] = (int)ARGUMENTS.GetValue(i);
			break;
			
		// FFT
		case "/muse/elements/raw_fft0":
			for (int i = 0; i < 129; i++)
				Raw_fft0[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/raw_fft1":
			for (int i = 0; i < 129; i++)
				Raw_fft1[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/raw_fft2":
			for (int i = 0; i < 129; i++)
				Raw_fft2[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/raw_fft3":
			for (int i = 0; i < 129; i++)
				Raw_fft3[i] = (float)ARGUMENTS.GetValue(i);
			break;
			
		// Absolute Band power
		case "/muse/elements/low_freqs_absolute":
			for (int i = 0; i < 4; i++)
				Absolute_low_freqs[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/delta_absolute":
			for (int i = 0; i < 4; i++)
				Absolute_delta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/theta_absolute":
			for (int i = 0; i < 4; i++)
				Absolute_theta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/alpha_absolute":
			for (int i = 0; i < 4; i++)
				Absolute_alpha[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/beta_absolute":
			for (int i = 0; i < 4; i++)
				Absolute_beta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/gamma_absolute":
			for (int i = 0; i < 4; i++)
				Absolute_gamma[i] = (float)ARGUMENTS.GetValue(i);
			break;
			
		// Relative Band power	
		case "/muse/elements/delta_relative":
			for (int i = 0; i < 4; i++)
				Relative_delta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/theta_relative":
			for (int i = 0; i < 4; i++)
				Relative_theta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/alpha_relative":
			for (int i = 0; i < 4; i++)
				Relative_alpha[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/beta_relative":
			for (int i = 0; i < 4; i++)
				Relative_beta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/gamma_relative":
			for (int i = 0; i < 4; i++)
				Relative_gamma[i] = (float)ARGUMENTS.GetValue(i);
			break;
			
		// Band power Score
		case "/muse/elements/delta_session_score":
			for (int i = 0; i < 4; i++)
				Score_delta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/theta_session_score":
			for (int i = 0; i < 4; i++)
				Score_theta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/alpha_session_score":
			for (int i = 0; i < 4; i++)
				Score_alpha[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/beta_session_score":
			for (int i = 0; i < 4; i++)
				Score_beta[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/gamma_session_score":
			for (int i = 0; i < 4; i++)
				Score_gamma[i] = (float)ARGUMENTS.GetValue(i);
			break;
			
		// HeadBand status
		case "/muse/elements/touching_forehead":
			Headband_Forehead = (int)ARGUMENTS.GetValue(0);
			break;
		case "/muse/elements/horseshoe":
			for (int i = 0; i < 4; i++)
				Headband[i] = (float)ARGUMENTS.GetValue(i);
			break;
		case "/muse/elements/is_good":
			for (int i = 0; i < 4; i++)
				Headband_Strict[i] = (int)ARGUMENTS.GetValue(i);
			break;
			
		// Muscle movements
		case "/muse/elements/blink":
			Blink = (int)ARGUMENTS.GetValue(0);
			break;
		case "/muse/elements/jaw_clench":
			JawClench = (int)ARGUMENTS.GetValue(0);
			break;
			
		// Experimental
		case "/muse/elements/experimental/concentration":
			Concentration = (float)ARGUMENTS.GetValue(0);
			break;
		case "/muse/elements/experimental/mellow":
			Mellow = (float)ARGUMENTS.GetValue(0);
			break;
			
		// Battery infos
		case "/muse/batt":
			for (int i = 0; i < 4; i++)
				Batt[i] = (int)ARGUMENTS.GetValue(i);
			break;
			
		// DRL/Ref
		case "/muse/drlref":
			for (int i = 0; i < 2; i++)
				DRLRef[i] = (float)ARGUMENTS.GetValue(i);
			break;
			
		// Config
		case "/muse/config":
			Config = JsonUtility.FromJson<MuseConfig>((string)ARGUMENTS[0]);
			break;
		
		default:
			break;
		}
	}
}