using UnityEngine;
using System.Collections;

public class FFTVisualizer : MonoBehaviour {
	
	const int NumberOfValues = 129;
	
	[SerializeField] float MaxValue = 40f;
	[SerializeField] float MinValue = -20f;
	
	float[] Histogram;
	
	[SerializeField] Vector2 m_position;
	[Range(0,2000)]
	[SerializeField] float m_width;
	[Range(0,1000)]
	[SerializeField] float m_height;
	
	[Range(0,3)]
	[SerializeField] int m_channel;
	
	Material m_lineMaterial;
	[SerializeField] Color m_lineColor;
	[SerializeField] Color m_boxColor;
	
	public void Initialize()
	{
		SetChannel(0);
	}
	
	public void SetChannel(int Channel)
	{
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
	}
	
	public int GetChannel()
	{
		return m_channel;
	}
	
	public void Resize(Vector2 Position, float Width, float Height)
	{
		m_position = Position;
		m_width = Width;
		m_height = Height;
	}
	
	void Start()
	{
		m_lineMaterial = Resources.Load<Material>("Material/GL_Line");
	}
	
	public void Draw()
	{
		if (Event.current.type == EventType.Repaint)
		{
			try
			{
				Histogram = MUSE_DATAS.Instance.Raw_fft[m_channel];
				
				float WidthRatio = (float)(m_width-1) / (NumberOfValues-1);
				float HeightRatio = m_height / (MaxValue - MinValue);
				Vector2 LevelingOffset = new Vector2(m_position.x - m_width/2, m_position.y - ((MaxValue - MinValue)/2 * HeightRatio));
				
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
				
				// Histogram
				
				GL.Color (m_lineColor);
				for (int i = 0; i < NumberOfValues-1; i++)
				{
					GL.Vertex(new Vector2((i) * WidthRatio,		(Histogram[i] - MinValue) * HeightRatio) + LevelingOffset);
					GL.Vertex(new Vector2((i+1) * WidthRatio,	(Histogram[i+1] - MinValue) * HeightRatio) + LevelingOffset);
					
					if(Histogram[i] > MaxValue)
						MaxValue = Histogram[i];
					else if(Histogram[i] < MinValue)
						MinValue = Histogram[i];
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
