using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Algorithms : MonoBehaviour {
	
	[SerializeField] FloatCallback ConcentrationCallback;
	[SerializeField] FloatCallback MellowCallback;
	
	public bool Focused = false;
	public bool Relaxed = false;
	
	public float FocusedValue;
	public float RelaxedValue;
	
	[SerializeField] float VariationSpeed;
	
	float low;
	float high;
	int Muse_data_log_Lenght = 20;
	float[] Muse_data_log;
	
	void Start()
	{
		Init(float.MinValue);
		InvokeRepeating("Analyse", 1,1);
	}
	
	void Update()
	{
		FocusedValue = Mathf.MoveTowards(FocusedValue, Focused ? 1 : 0, Time.deltaTime * VariationSpeed);
		RelaxedValue = Mathf.MoveTowards(RelaxedValue, Relaxed ? 1 : 0, Time.deltaTime * VariationSpeed);
		
		ConcentrationCallback.Invoke(Focused ? 1 : 0);
		MellowCallback.Invoke(RelaxedValue);
	}
	
	public void AddCallBacks(UnityAction<float> Concentration, UnityAction<float> Mellow)
	{
		ConcentrationCallback.AddListener(Concentration);
		MellowCallback.AddListener(Mellow);
	}
	
	public void Muse_fft_alpha_all_median(float[][] INPUT)
	{
		float[] fft_all = new float[INPUT[0].Length];
		
		for (int i = 1; i < INPUT[0].Length; i++)
			fft_all[i] = (INPUT[0][i]/2 + INPUT[1][i] + INPUT[2][i] + INPUT[3][i]/2) /3;
			
		float HZperBin = 220/256f;
		int Alpha_search_min = Mathf.FloorToInt(8/HZperBin)+1;
		int Alpha_search_max = Mathf.CeilToInt(12/HZperBin)+1;
		
		float Alpha_max = float.MinValue;
		int Alpha_max_index = 0;
		
		for (int i = Alpha_search_min; i < Alpha_search_max; i++)
			if(fft_all[i] > Alpha_max)
			{
				Alpha_max = fft_all[i];
				Alpha_max_index = i;
			}
		
		float Alpha_max_HZ = (Alpha_max_index -1)*HZperBin;
		
		if(Alpha_max_HZ <= low)
			low = Alpha_max_HZ;
		
		if(Alpha_max_HZ >= high)
			high = Alpha_max_HZ;
		
		Muse_data_log[Mathf.FloorToInt(Alpha_max_HZ)] += 1;
	}
	
	void Init(float INPUT)
	{
		low = -INPUT;
		high = INPUT;
		Muse_data_log = new float[Muse_data_log_Lenght];
	}
	
	void Analyse()
	{
		Relaxed = false;
		Focused = false;
		
		float Muse_data_count = Muse_data_log[7] +
								Muse_data_log[8] +
								Muse_data_log[9] + 
								Muse_data_log[10] + 
								Muse_data_log[11];
		
		float State_Relaxed_cumul = Muse_data_log[7] +
									Muse_data_log[11];
		
		if(State_Relaxed_cumul*100/Muse_data_count > 50)
			Relaxed = true;
		
		float State_Concentrated_cumul = Muse_data_log[8] +
											Muse_data_log[9] + 
											Muse_data_log[10];
		
		if(State_Concentrated_cumul*100/Muse_data_count > 30)
			Focused = true;
		
		Muse_data_log = new float[Muse_data_log_Lenght];
	}
}
