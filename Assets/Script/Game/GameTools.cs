using UnityEngine;

public static class GameTools
{
    public static void ChangeLayer(GameObject obj, string layerName)
    {
        obj.layer = LayerMask.NameToLayer(layerName);
        Transform tf = obj.transform;
        for (int i = 0, count = tf.childCount; i < count; i++)
        {
            ChangeLayer(tf.GetChild(i).gameObject, layerName);
        }
    }
}
