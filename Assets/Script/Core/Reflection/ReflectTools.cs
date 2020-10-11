using System.Reflection;
using UnityEngine;

public static class ReflectTools
{
	public static FieldInfo SetValue(object obj, string name, object value)
	{
		FieldInfo field = obj.GetType().GetField(name);
		if (field == null)
		{
			Debug.LogError(obj.GetType() + "类型不存在成员" + name);
			return null;
		}
		field.SetValue(obj, value);
		return field;
	}
}
