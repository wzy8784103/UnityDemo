using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class LogHelper : SingletonBase<LogHelper>
{
	string outPath;
	//private StreamWriter writer;
	public override void InitSingleton()
	{
		//string outPath = "";
		if(PlatformManager.IsEditor())
		{
			outPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/outLog.txt";
		}
		else
		{
			outPath = Application.persistentDataPath + "/outLog.txt";
		}
		if(File.Exists(outPath))
		{
			File.Delete(outPath);
		}
		Application.logMessageReceived += OnLogMessage;
	}

	private void OnLogMessage(string condition, string stackTrace, LogType type)
	{
		using (StreamWriter writer = new StreamWriter(outPath, true, Encoding.UTF8))
		{
			writer.WriteLine(DateTime.Now + " condition:" + condition);
			writer.WriteLine("stackTrace:" + stackTrace);
		}
	}

	public static void Log<T>(List<T> list, string frontStr = "")
	{
		if(!Debug.unityLogger.logEnabled)
		{
			return;
		}
		string result = "";
		foreach(var item in list)
		{
			result += (item.ToString() + ", ");
		}
		Debug.Log(frontStr + result);
	}
	public static void Log<TKey, TValue>(Dictionary<TKey, TValue> dic, string frontStr = "")
	{
		if (!Debug.unityLogger.logEnabled)
		{
			return;
		}
		string result = "";
		foreach (var kv in dic)
		{
			result += ("key:" + kv.Key.ToString() + " value:" + kv.Value.ToString() + ", ");
		}
		Debug.Log(frontStr + result);
	}
}
