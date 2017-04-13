using UnityEngine;

using SharpOSC;

using System.Collections;
using System.Diagnostics;

/*
Launch muse-io and store datas to MUSE_DATAS
Works with : muse-io 3.6.5 (Build-17 Jan 30 2015 20:10:03)
*/

public class MUSE_IO_READER : MonoBehaviour
{
	[Header("Paramètres de connexion")]
	[SerializeField]
	int Port;
	
	[HideInInspector] public bool IsRunning = false;
	
	MUSE_DEBUG DebugConsole;
	UDPListener listener;
	Process MUSE_CMD;

	void Start()
	{
		// Init muse-io
		
		MUSE_CMD = new Process();

		MUSE_CMD.StartInfo.FileName = "muse-io";
		MUSE_CMD.StartInfo.Arguments = "--osc osc.udp://127.0.0.1:" + Port + " --osc-timestamp";
		MUSE_CMD.StartInfo.RedirectStandardOutput = true;
		MUSE_CMD.StartInfo.UseShellExecute = false;
		MUSE_CMD.StartInfo.CreateNoWindow = true;

		// Inits
		
		DebugConsole = GetComponent<MUSE_DEBUG>();
	}
	
	public void Launch()
	{
		MUSE_CMD.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(CmdOutputHandler);

		try
		{
			print("Starting muse-io process...");
			MUSE_CMD.Start();

			print("muse-io started !\nReading output...");
			MUSE_CMD.BeginOutputReadLine();

			IsRunning = true;
		}
		catch (System.Exception ex)
		{
			print(ex);
		}
	}
	
	void OnDisable()
	{
		if(IsRunning)
		{

			if (MUSE_CMD.CloseMainWindow ()) {
				print ("muse-io stopped.");
				MUSE_CMD.Close(); // free ressources
			}
			
			if(listener != null)
			{
				listener.Dispose();
				print("Listener closed.");
			}
		}
	}
	
	void Receive(OscPacket packet)
	{
		OscMessage messageReceived = (OscMessage)packet;
		MUSE_DATAS.Instance.DataAttribution(messageReceived.Address, messageReceived.Arguments.ToArray());
	}
	
	void CmdOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
	{
		if (!string.IsNullOrEmpty(outLine.Data))
		{
			if(outLine.Data.Contains("Connected."))
				listener = new UDPListener(Port, Receive);
			
			DebugConsole.WriteLine(outLine.Data);
		}
	}
}
