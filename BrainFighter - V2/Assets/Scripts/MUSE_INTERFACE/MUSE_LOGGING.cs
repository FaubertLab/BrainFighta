using UnityEngine;
using System.Collections;
using System.IO;

public class MUSE_LOGGING : MonoBehaviour {
	
	[SerializeField] string m_Directory;
	
	StreamWriter m_File;
	
	public void CreateNewLog()
	{
		System.IO.Directory.CreateDirectory(@m_Directory);
		m_File = System.IO.File.AppendText(@m_Directory + "\\" + System.DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss") + ".csv");
	}
	
	public void Log(string INPUT)
	{
		m_File.WriteLine(INPUT);
	}
	
	void OnDisable()
	{
		if(m_File != null)
			m_File.Dispose();
	}
}
