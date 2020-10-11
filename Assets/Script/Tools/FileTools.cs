using System.IO;
using UnityEngine;

public static class FileTools
{
	public static void WriteAllText(string source, string content)
	{
		File.WriteAllText(source, content);
	}
	public static void Copy(string source, string dest)
	{
		if(!File.Exists(source))
		{
			Debug.LogError(source + "不存在");
			return;
		}
		File.Copy(source, dest, true);
	}
}
