using UnityEngine;
using System.Collections;

public class ShowFPS : MonoBehaviour
{
    public float updateInterval = 0.5F;
    private float lastInterval;
    private int frames = 0;
    private float fps;

    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }

    void OnGUI()
    {
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;
        bb.normal.textColor = Color.white;
        bb.fontSize = 30;
        GUI.Label(new Rect(0, 80, 200, 200), "FPS:" + fps.ToString("f2"), bb);
    }

    void Update()
    {
        frames++;
        if (Time.realtimeSinceStartup > lastInterval + updateInterval)
        {
            fps = frames / (Time.realtimeSinceStartup - lastInterval);
            frames = 0;
            lastInterval = Time.realtimeSinceStartup;
        }
    }
}