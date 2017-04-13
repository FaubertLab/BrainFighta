using UnityEngine;
using System.Collections.Generic;
using System.Linq;
 
public class EEGVisualizer : MonoBehaviour {
	
	const float MaxValue = 1682.815f;
	
	[SerializeField] Vector2 m_position;
	[Range(0,1000)]
	[SerializeField] int m_resolution = 500;
	[Range(0,2000)]
	[SerializeField] float m_width;
	[Range(0,1000)]
	[SerializeField] float m_height;
	
	[Range(0,3)]
	[SerializeField] int m_channel;
	
	Queue<float> m_history;
	
	Material m_lineMaterial;
	[SerializeField] Color m_lineColor;
	[SerializeField] Color m_boxColor;
	
	public void Initialize(int Resolution, int Channel)
	{
		MUSE_DATAS.Instance.SubscribeVisualizers(this);
		
		m_resolution = Resolution;
		m_channel = Channel;
		
		switch (Channel)
		{
		case 0:
			m_lineColor = Color.blue;
			break;
		case 1:
			m_lineColor = Color.cyan;
			break;
		case 2:
			m_lineColor = Color.yellow;
			break;
		case 3:
			m_lineColor = Color.magenta;
			break;
		default:
			break;
		}
		
		m_history = new Queue<float>();
		
		for (int i = 0; i < m_resolution; i++)
			m_history.Enqueue(MaxValue/2);
	}
	
	public void Resize(Vector2 Position, float Width, float Height)
	{
		m_position = Position;
		m_width = Width;
		m_height = Height;
	}
	
	void Start()
	{
		MUSE_DATAS.Instance.SubscribeVisualizers(this);
		
		m_lineMaterial = Resources.Load<Material>("Material/GL_Line");
		
		m_history = new Queue<float>();
		
		for (int i = 0; i < m_resolution; i++)
			m_history.Enqueue(MaxValue/2);
	}
	 
	public void UpdateHistory(float[] INPUT)
	{
		m_history.Enqueue(INPUT[m_channel]);
			
		while (m_history.Count > m_resolution)
			m_history.Dequeue();
	}
	 
	public void Draw()
	{
		if (Event.current.type == EventType.Repaint)
		{
			try
			{
				float[] HistoryArray = m_history.ToArray();
				
				float WidthRatio = (float)(m_width-1) / (m_resolution-1);
				float HeightRatio = m_height / MaxValue;
				Vector2 LevelingOffset = new Vector2(m_position.x - m_width/2, m_position.y - (MaxValue/2 * HeightRatio));
				
				GL.PushMatrix ();
				m_lineMaterial.SetPass(0);
				GL.LoadPixelMatrix(0, Screen.width, 0, Screen.height);
				
				GL.Begin(GL.LINES);
				
				// Cadre
				
				GL.Color (m_boxColor);
			 
				GL.Vertex(new Vector2(m_position.x - m_width/2,	m_position.y + m_height/2));
				GL.Vertex(new Vector2(m_position.x + m_width/2,	m_position.y + m_height/2));
				
				GL.Vertex(new Vector2(m_position.x + m_width/2,	m_position.y + m_height/2));
				GL.Vertex(new Vector2(m_position.x + m_width/2,	m_position.y - m_height/2));
				
				GL.Vertex(new Vector2(m_position.x + m_width/2,	m_position.y - m_height/2));
				GL.Vertex(new Vector2(m_position.x - m_width/2,	m_position.y - m_height/2));
				
				GL.Vertex(new Vector2(m_position.x - m_width/2,	m_position.y - m_height/2));
				GL.Vertex(new Vector2(m_position.x - m_width/2,	m_position.y + m_height/2));
				
				// Plot
				
				GL.Color (m_lineColor);
				for (int i = 0; i < m_history.Count - 1; i++)
				{
					GL.Vertex(new Vector2((i) * WidthRatio,		HistoryArray[i] * HeightRatio) + LevelingOffset);
					GL.Vertex(new Vector2((i+1) * WidthRatio,	HistoryArray[i+1] * HeightRatio) + LevelingOffset);
				}
				
				GL.End();
				GL.PopMatrix();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e);
			}
		}
	}
}